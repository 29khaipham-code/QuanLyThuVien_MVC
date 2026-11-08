using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.DocGia;
using QuanLyThuVien.ViewModels;

namespace QuanLyThuVien.Services;

// Sinh thông báo từ dữ liệu mượn trả, đặt trước, sách điện tử, tiền phạt và hạn thẻ.
// Mỗi sự kiện có một "khóa chống trùng" nên quét bao nhiêu lần cũng chỉ tạo một thông báo.
public class ThongBaoService
{
    private const int SoNgayBaoSapDenHan = 3;     // báo khi còn <= 3 ngày tới hạn trả
    private const int SoGioBaoGiuCho = 24;        // báo khi giữ chỗ còn <= 24 giờ
    private const int SoNgayBaoQuyenDoc = 2;      // báo khi quyền đọc còn <= 2 ngày
    private const int SoNgayBaoThe = 7;           // báo khi thẻ còn <= 7 ngày
    private const int SoPhutGiuaHaiLanQuet = 2;   // quét theo yêu cầu tối đa 2 phút một lần cho mỗi độc giả
    private const int KichThuocLo = 500;

    private readonly QuanLyThuVienContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ThongBaoService> _logger;

    private sealed record ThongBaoMoi(
        int DocGiaId, string Loai, string KhoaTrung, string TieuDe, string NoiDung, string? Url);

    public ThongBaoService(
        QuanLyThuVienContext db,
        IMemoryCache cache,
        ILogger<ThongBaoService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    // Quét cho một độc giả nhưng không quá thường xuyên (gọi mỗi lần tải trang cũng không sao)
    public async Task QuetNeuCanAsync(int docGiaId)
    {
        var khoa = $"thongbao-quet:{docGiaId}";
        if (_cache.TryGetValue(khoa, out _)) return;

        _cache.Set(khoa, true, TimeSpan.FromMinutes(SoPhutGiuaHaiLanQuet));

        try
        {
            await TaoThongBaoAsync(docGiaId);
        }
        catch (Exception ex)
        {
            // Lỗi sinh thông báo không được làm hỏng trang đang xem
            _logger.LogWarning(ex, "Không quét được thông báo cho độc giả {DocGiaId}", docGiaId);
        }
    }

    // Sinh thông báo cho một độc giả (docGiaId) hoặc cho mọi độc giả đang hoạt động (null).
    // Trả về số thông báo mới được tạo.
    public async Task<int> TaoThongBaoAsync(int? docGiaId = null)
    {
        var moi = new List<ThongBaoMoi>();

        moi.AddRange(await SachMuonAsync(docGiaId));
        moi.AddRange(await DatTruocAsync(docGiaId));
        moi.AddRange(await QuyenDocAsync(docGiaId));
        moi.AddRange(await TienPhatAsync(docGiaId));
        moi.AddRange(await TheSapHetHanAsync(docGiaId));

        return await LuuAsync(moi);
    }

    
    // CÁC NGUỒN SỰ KIỆN
    

    // Sắp đến hạn trả và quá hạn trả (cùng một truy vấn)
    private async Task<List<ThongBaoMoi>> SachMuonAsync(int? docGiaId)
    {
        var homNay = DateOnly.FromDateTime(DateTime.Today);
        var han = homNay.AddDays(SoNgayBaoSapDenHan);

        var query = _db.ChiTietMuons.AsNoTracking()
            .Where(c => c.NgayTra == null
                     && c.PhieuMuon.TrangThai != "Huy"
                     && c.PhieuMuon.DocGia.TrangThai == "HoatDong"
                     && c.HanTra <= han);

        if (docGiaId.HasValue)
            query = query.Where(c => c.PhieuMuon.DocGiaId == docGiaId.Value);

        var ds = await query
            .Select(c => new
            {
                DocGiaId = c.PhieuMuon.DocGiaId,
                c.Id,
                TieuDe = c.BanSao.Sach.TieuDe,
                c.HanTra
            })
            .ToListAsync();

        var ketQua = new List<ThongBaoMoi>();

        foreach (var c in ds)
        {
            var soNgay = c.HanTra.DayNumber - homNay.DayNumber;   // âm = đã quá hạn

            if (soNgay < 0)
            {
                ketQua.Add(new ThongBaoMoi(
                    c.DocGiaId, LoaiThongBao.QuaHan, $"QuaHan:{c.Id}",
                    "Sách đã quá hạn trả",
                    Cat($"Cuốn \"{c.TieuDe}\" đã quá hạn {-soNgay} ngày (hạn trả {c.HanTra:dd/MM/yyyy}). " +
                        "Vui lòng mang trả sớm để tránh phát sinh thêm tiền phạt.", 500),
                    "/DocGia/MuonTra"));
            }
            else
            {
                var thoiGian = soNgay == 0 ? "đến hạn trả vào hôm nay" : $"còn {soNgay} ngày là đến hạn trả";
                ketQua.Add(new ThongBaoMoi(
                    c.DocGiaId, LoaiThongBao.SapDenHan, $"SapDenHan:{c.Id}",
                    "Sắp đến hạn trả sách",
                    Cat($"Cuốn \"{c.TieuDe}\" {thoiGian} (hạn trả {c.HanTra:dd/MM/yyyy}).", 500),
                    "/DocGia/MuonTra"));
            }
        }

        return ketQua;
    }

    // Sách đặt trước đã sẵn sàng và giữ chỗ sắp hết hạn
    private async Task<List<ThongBaoMoi>> DatTruocAsync(int? docGiaId)
    {
        var bayGio = DateTime.Now;

        var query = _db.DatTruocs.AsNoTracking()
            .Where(d => d.TrangThai == "DaGiu"
                     && d.DocGia.TrangThai == "HoatDong"
                     && (d.NgayHetHan == null || d.NgayHetHan > bayGio));

        if (docGiaId.HasValue)
            query = query.Where(d => d.DocGiaId == docGiaId.Value);

        var ds = await query
            .Select(d => new
            {
                d.DocGiaId,
                d.Id,
                TieuDe = d.Sach.TieuDe,
                d.NgayHetHan,
                ViTri = d.BanSaoGiu != null ? d.BanSaoGiu.ViTri : null
            })
            .ToListAsync();

        var ketQua = new List<ThongBaoMoi>();

        foreach (var d in ds)
        {
            var hanNhan = d.NgayHetHan.HasValue ? $" Vui lòng đến nhận trước {d.NgayHetHan:HH:mm dd/MM/yyyy}." : "";
            var viTri = string.IsNullOrEmpty(d.ViTri) ? "" : $" Vị trí: {d.ViTri}.";

            ketQua.Add(new ThongBaoMoi(
                d.DocGiaId, LoaiThongBao.CoSachNhan, $"CoSachNhan:{d.Id}",
                "Sách đặt trước đã sẵn sàng",
                Cat($"Cuốn \"{d.TieuDe}\" bạn đặt trước đã có sách.{hanNhan}{viTri}", 500),
                "/DocGia/DatTruoc"));

            if (d.NgayHetHan.HasValue && d.NgayHetHan.Value <= bayGio.AddHours(SoGioBaoGiuCho))
            {
                var conLai = d.NgayHetHan.Value - bayGio;
                var thoiGian = conLai.TotalHours >= 1
                    ? $"{(int)conLai.TotalHours} giờ"
                    : $"{Math.Max(1, (int)conLai.TotalMinutes)} phút";

                ketQua.Add(new ThongBaoMoi(
                    d.DocGiaId, LoaiThongBao.GiuChoSapHet, $"GiuChoSapHet:{d.Id}",
                    "Sắp hết hạn giữ chỗ",
                    Cat($"Chỉ còn khoảng {thoiGian} để nhận cuốn \"{d.TieuDe}\". " +
                        "Quá hạn, yêu cầu đặt trước sẽ bị hủy.", 500),
                    "/DocGia/DatTruoc"));
            }
        }

        return ketQua;
    }

    // Quyền đọc sách điện tử sắp hết hạn
    private async Task<List<ThongBaoMoi>> QuyenDocAsync(int? docGiaId)
    {
        var bayGio = DateTime.Now;

        var query = _db.LuotDocSachDienTus.AsNoTracking()
            .Where(l => l.TrangThai == "HieuLuc"
                     && l.DocGia.TrangThai == "HoatDong"
                     && l.NgayHetHan > bayGio
                     && l.NgayHetHan <= bayGio.AddDays(SoNgayBaoQuyenDoc));

        if (docGiaId.HasValue)
            query = query.Where(l => l.DocGiaId == docGiaId.Value);

        var ds = await query
            .Select(l => new { l.DocGiaId, l.Id, TieuDe = l.TepSach.Sach.TieuDe, l.NgayHetHan })
            .ToListAsync();

        return ds.Select(l => new ThongBaoMoi(
                l.DocGiaId, LoaiThongBao.QuyenDocSapHet, $"QuyenDocSapHet:{l.Id}",
                "Quyền đọc sách điện tử sắp hết hạn",
                Cat($"Quyền đọc cuốn \"{l.TieuDe}\" sẽ hết hạn lúc {l.NgayHetHan:HH:mm dd/MM/yyyy}. " +
                    "Hết hạn bạn cần đăng ký đọc lại.", 500),
                "/DocGia/SachDienTu/CuaToi"))
            .ToList();
    }

    // Khoản phạt chưa thanh toán
    private async Task<List<ThongBaoMoi>> TienPhatAsync(int? docGiaId)
    {
        var query = _db.Phats.AsNoTracking()
            .Where(p => !p.DaThanhToan
                     && p.ChiTietMuon.PhieuMuon.DocGia.TrangThai == "HoatDong");

        if (docGiaId.HasValue)
            query = query.Where(p => p.ChiTietMuon.PhieuMuon.DocGiaId == docGiaId.Value);

        var ds = await query
            .Select(p => new
            {
                DocGiaId = p.ChiTietMuon.PhieuMuon.DocGiaId,
                p.Id,
                p.LoaiPhat,
                p.SoTien,
                TieuDe = p.ChiTietMuon.BanSao.Sach.TieuDe
            })
            .ToListAsync();

        return ds.Select(p =>
        {
            var loai = p.LoaiPhat switch
            {
                "TraMuon" => "trả muộn",
                "HuHong" => "làm hư hỏng",
                "MatSach" => "làm mất sách",
                _ => p.LoaiPhat
            };

            return new ThongBaoMoi(
                p.DocGiaId, LoaiThongBao.PhatMoi, $"PhatMoi:{p.Id}",
                "Bạn có khoản phạt chưa thanh toán",
                Cat($"Khoản phạt {loai} {HienThi.Tien(p.SoTien)} cho cuốn \"{p.TieuDe}\". " +
                    "Vui lòng nộp tại quầy thủ thư.", 500),
                "/DocGia/MuonTra/Phat");
        })
            .ToList();
    }

    // Thẻ thư viện sắp hết hạn
    private async Task<List<ThongBaoMoi>> TheSapHetHanAsync(int? docGiaId)
    {
        var homNay = DateOnly.FromDateTime(DateTime.Today);
        var han = homNay.AddDays(SoNgayBaoThe);

        var query = _db.DocGia.AsNoTracking()
            .Where(d => d.TrangThai == "HoatDong" && d.NgayHetHan >= homNay && d.NgayHetHan <= han);

        if (docGiaId.HasValue)
            query = query.Where(d => d.Id == docGiaId.Value);

        var ds = await query.Select(d => new { d.Id, d.NgayHetHan }).ToListAsync();

        // Khóa có kèm ngày hết hạn: sau khi gia hạn thẻ, lần sắp hết hạn tiếp theo sẽ báo lại
        return ds.Select(d => new ThongBaoMoi(
                d.Id, LoaiThongBao.TheSapHetHan, $"TheSapHetHan:{d.Id}:{d.NgayHetHan:yyyyMMdd}",
                "Thẻ thư viện sắp hết hạn",
                $"Thẻ thư viện của bạn sẽ hết hạn vào {d.NgayHetHan:dd/MM/yyyy}. Vui lòng liên hệ thủ thư để gia hạn.",
                "/DocGia/HoSo"))
            .ToList();
    }

    // =====================================================
    // LƯU (bỏ qua các thông báo đã có)
    // =====================================================
    private async Task<int> LuuAsync(List<ThongBaoMoi> moi)
    {
        if (moi.Count == 0) return 0;

        var soLuong = 0;

        foreach (var lo in moi.Chunk(KichThuocLo))
        {
            var docGiaIds = lo.Select(x => x.DocGiaId).Distinct().ToList();
            var khoas = lo.Select(x => x.KhoaTrung).Distinct().ToList();

            var daCo = await _db.ThongBaos.AsNoTracking()
                .Where(t => docGiaIds.Contains(t.DocGiaId) && khoas.Contains(t.KhoaTrung))
                .Select(t => new { t.DocGiaId, t.KhoaTrung })
                .ToListAsync();

            var daCoSet = daCo.Select(x => (x.DocGiaId, x.KhoaTrung)).ToHashSet();
            var can = lo.Where(x => !daCoSet.Contains((x.DocGiaId, x.KhoaTrung))).ToList();

            if (can.Count == 0) continue;

            _db.ThongBaos.AddRange(can.Select(ToEntity));

            try
            {
                await _db.SaveChangesAsync();
                soLuong += can.Count;
            }
            catch (DbUpdateException)
            {
                // Hai lần quét chạy cùng lúc nên bị trùng khóa: thử lại từng dòng và bỏ qua dòng đã có.
                // (Chỉ gọi ở nơi không có thay đổi nào khác chưa lưu trong cùng DbContext.)
                _db.ChangeTracker.Clear();

                foreach (var x in can)
                {
                    _db.ThongBaos.Add(ToEntity(x));

                    try
                    {
                        await _db.SaveChangesAsync();
                        soLuong++;
                    }
                    catch (DbUpdateException)
                    {
                        _db.ChangeTracker.Clear();
                    }
                }
            }
        }

        return soLuong;
    }

    private static ThongBao ToEntity(ThongBaoMoi x) => new()
    {
        DocGiaId = x.DocGiaId,
        Loai = x.Loai,
        KhoaTrung = x.KhoaTrung,
        TieuDe = x.TieuDe,
        NoiDung = x.NoiDung,
        Url = x.Url,
        NgayTao = DateTime.Now
    };

    // Cột NoiDung dài tối đa 500 ký tự
    private static string Cat(string s, int toiDa) =>
        s.Length <= toiDa ? s : s[..(toiDa - 1)] + "…";
}
