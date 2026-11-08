using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Areas.DocGia.Helpers;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.DocGia;
using System.Security.Cryptography;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public class SachDienTuController : DocGiaControllerBase
    {
        private const int KichThuocTrang = 8;
        private const int SoQuyenDocToiDa = 5;      // số quyền đọc còn hiệu lực cùng lúc
        private const int SoPhutHieuLucToken = 10;  // thời hạn của đường dẫn mở tệp
        private const int SoDongLichSu = 20;

        private readonly IWebHostEnvironment _env;
        private readonly ITimeLimitedDataProtector _protector;

        public SachDienTuController(
            QuanLyThuVienContext db,
            IWebHostEnvironment env,
            IDataProtectionProvider dataProtection) : base(db)
        {
            _env = env;
            _protector = dataProtection
                .CreateProtector("QuanLyThuVien.SachDienTu.Tep")
                .ToTimeLimitedDataProtector();
        }

        // =====================================================
        // TAB 1: THƯ VIỆN ĐIỆN TỬ (danh mục sách có tệp điện tử)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa, int? theLoaiId, string? sapXep = "moi", int trang = 1)
        {
            tuKhoa = tuKhoa?.Trim();
            var homNay = DateOnly.FromDateTime(DateTime.Today);

            var query = Db.Saches.AsNoTracking()
                .Where(s => s.TrangThai == "HoatDong"
                         && s.TepSachDienTus.Any(t => t.TrangThai == "HoatDong"
                                                   && (t.NgayHetBanQuyen == null || t.NgayHetBanQuyen >= homNay)));

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(s =>
                    s.TieuDe.Contains(tuKhoa) ||
                    s.TacGia.Any(t => t.TenTacGia.Contains(tuKhoa)));
            }

            if (theLoaiId.HasValue)
                query = query.Where(s => s.TheLoaiId == theLoaiId.Value);

            query = sapXep switch
            {
                "ten" => query.OrderBy(s => s.TieuDe),
                "nam" => query.OrderByDescending(s => s.NamXuatBan).ThenBy(s => s.TieuDe),
                _ => query.OrderByDescending(s => s.Id)
            };

            var tongSo = await query.CountAsync();
            var tongTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)KichThuocTrang));
            trang = Math.Clamp(trang, 1, tongTrang);

            var items = await query
                .Skip((trang - 1) * KichThuocTrang)
                .Take(KichThuocTrang)
                .ToItemViewModel()
                .ToListAsync();

            var vm = new SachIndexViewModel
            {
                TuKhoa = tuKhoa,
                TheLoaiId = theLoaiId,
                SapXep = sapXep ?? "moi",
                Trang = trang,
                KichThuocTrang = KichThuocTrang,
                TongSo = tongSo,
                Items = items
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DanhSach", vm);

            vm.TheLoais = await Db.TheLoais.AsNoTracking()
                .OrderBy(t => t.TenTheLoai)
                .Select(t => new LuaChonViewModel { Id = t.Id, Ten = t.TenTheLoai })
                .ToListAsync();

            return View(vm);
        }

        // =====================================================
        // TAB 2: SÁCH CỦA TÔI (các quyền đọc)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> CuaToi()
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            await HetHanQuyenAsync(dg.Id);

            var tatCa = await Db.LuotDocSachDienTus.AsNoTracking()
                .Where(l => l.DocGiaId == dg.Id)
                .OrderByDescending(l => l.NgayBatDau)
                .Select(l => new QuyenDocItemViewModel
                {
                    LuotDocId = l.Id,
                    SachId = l.TepSach.SachId,
                    TieuDe = l.TepSach.Sach.TieuDe,
                    DinhDang = l.TepSach.DinhDang,
                    CheDoTruyCap = l.TepSach.CheDoTruyCap,
                    TrangThai = l.TrangThai,
                    NgayBatDau = l.NgayBatDau,
                    NgayHetHan = l.NgayHetHan,
                    LanTruyCapCuoi = l.LanTruyCapCuoi,
                    TepKhaDung = l.TepSach.TrangThai == "HoatDong"
                })
                .ToListAsync();

            return View(new SachCuaToiViewModel
            {
                DangHieuLuc = tatCa.Where(q => q.ConHieuLuc).ToList(),
                LichSu = tatCa.Where(q => !q.ConHieuLuc).Take(SoDongLichSu).ToList(),
                ToiDa = SoQuyenDocToiDa,
                SoNgayDoc = dg.LoaiDocGia.SoNgayMuonToiDa
            });
        }

        // =====================================================
        // CẤP QUYỀN ĐỌC (bấm "Đọc online" ở trang chi tiết sách)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MuonDoc(int tepSachId)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            var tep = await Db.TepSachDienTus.AsNoTracking()
                .Include(t => t.Sach)
                .FirstOrDefaultAsync(t => t.Id == tepSachId);

            if (tep == null || tep.Sach.TrangThai != "HoatDong")
                return NotFound();

            IActionResult Loi(string thongBao)
            {
                TempData["Loi"] = thongBao;
                return RedirectToAction("ChiTiet", "Sach", new { area = "DocGia", id = tep.SachId });
            }

            var homNay = DateOnly.FromDateTime(DateTime.Today);

            if (dg.TrangThai != "HoatDong" || dg.NgayHetHan < homNay)
                return Loi("Thẻ thư viện của bạn đã hết hạn hoặc đang bị khóa, chưa thể đọc sách điện tử.");

            if (tep.TrangThai != "HoatDong")
                return Loi("Tệp sách này hiện không khả dụng.");

            if (tep.NgayHetBanQuyen.HasValue && tep.NgayHetBanQuyen.Value < homNay)
                return Loi("Tệp sách này đã hết bản quyền phát hành.");

            // Quyền đọc cũ đã quá hạn phải được đóng lại trước, để chỉ mục duy nhất cho phép tạo quyền mới
            await HetHanQuyenAsync(dg.Id);

            // Đã có quyền đang hiệu lực thì vào đọc luôn
            var dangCo = await Db.LuotDocSachDienTus
                .FirstOrDefaultAsync(l => l.DocGiaId == dg.Id
                                       && l.TepSachId == tepSachId
                                       && l.TrangThai == "HieuLuc");
            if (dangCo != null)
                return RedirectToAction(nameof(Doc), new { id = dangCo.Id });

            var soDangCo = await Db.LuotDocSachDienTus
                .CountAsync(l => l.DocGiaId == dg.Id && l.TrangThai == "HieuLuc");
            if (soDangCo >= SoQuyenDocToiDa)
                return Loi($"Bạn chỉ có thể giữ tối đa {SoQuyenDocToiDa} quyền đọc cùng lúc. Hãy chờ một quyền hết hạn rồi thử lại.");

            var bayGio = DateTime.Now;
            var luot = new LuotDocSachDienTu
            {
                DocGiaId = dg.Id,
                TepSachId = tepSachId,
                NgayBatDau = bayGio,
                NgayHetHan = bayGio.AddDays(dg.LoaiDocGia.SoNgayMuonToiDa),
                TrangThai = "HieuLuc"
            };
            Db.LuotDocSachDienTus.Add(luot);

            try
            {
                await Db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Loi("Không thể cấp quyền đọc lúc này. Có thể bạn vừa đăng ký cuốn này, vui lòng kiểm tra mục Sách của tôi.");
            }

            return RedirectToAction(nameof(Doc), new { id = luot.Id });
        }

        // =====================================================
        // TRANG ĐỌC SÁCH
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Doc(int id)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            var (luot, loi) = await KiemTraQuyenAsync(id, dg);
            if (luot == null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction(nameof(CuaToi));
            }

            var tep = luot.TepSach;

            // Ghi nhật ký "Mở" và thời điểm truy cập cuối
            luot.LanTruyCapCuoi = DateTime.Now;
            GhiNhatKy(luot.Id, "Mo");
            await Db.SaveChangesAsync();

            return View(new DocSachViewModel
            {
                LuotDocId = luot.Id,
                SachId = tep.SachId,
                TieuDe = tep.Sach.TieuDe,
                DinhDang = tep.DinhDang,
                CoTheXemTrucTuyen = tep.DinhDang.Equals("PDF", StringComparison.OrdinalIgnoreCase),
                CoTheTai = tep.CheDoTruyCap == "ChoPhepTai",
                NgayHetHan = luot.NgayHetHan,
                TokenXem = TaoToken(luot.Id, dg.Id)
            });
        }

        // =====================================================
        // PHỤC VỤ TỆP ĐỂ ĐỌC TRỰC TUYẾN (qua token ngắn hạn, không có đường dẫn công khai)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Tep(string token)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return Forbid();

            // Token chỉ dùng được bởi đúng độc giả đã nhận và trong thời gian ngắn
            if (string.IsNullOrEmpty(token) ||
                !GiaiMaToken(token, out var luotDocId, out var docGiaId) ||
                docGiaId != dg.Id)
            {
                return NotFound();
            }

            // Kiểm tra lại quyền ở mỗi lần mở tệp
            var (luot, _) = await KiemTraQuyenAsync(luotDocId, dg);
            if (luot == null) return NotFound();

            var tep = luot.TepSach;
            if (!tep.DinhDang.Equals("PDF", StringComparison.OrdinalIgnoreCase))
                return NotFound();

            var duongDan = LayDuongDanTep(tep.DuongDanTep);
            if (duongDan == null) return NotFound();

            // Trình xem PDF gửi nhiều yêu cầu theo từng đoạn (Range): chỉ ghi nhật ký ở yêu cầu đầu tiên
            if (!Request.Headers.ContainsKey("Range"))
            {
                GhiNhatKy(luot.Id, "Doc");
                await Db.SaveChangesAsync();
            }

            Response.Headers["Cache-Control"] = "no-store";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return PhysicalFile(duongDan, "application/pdf", enableRangeProcessing: true);
        }

        // =====================================================
        // TẢI XUỐNG (chỉ khi tệp ở chế độ ChoPhepTai)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Tai(int id)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            var (luot, loi) = await KiemTraQuyenAsync(id, dg);
            if (luot == null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction(nameof(CuaToi));
            }

            var tep = luot.TepSach;

            if (tep.CheDoTruyCap != "ChoPhepTai")
            {
                TempData["Loi"] = "Sách này chỉ cho đọc trực tuyến, không cho tải xuống.";
                return RedirectToAction(nameof(Doc), new { id });
            }

            var duongDan = LayDuongDanTep(tep.DuongDanTep);
            if (duongDan == null)
            {
                TempData["Loi"] = "Không tìm thấy tệp trên máy chủ. Vui lòng báo thủ thư.";
                return RedirectToAction(nameof(Doc), new { id });
            }

            GhiNhatKy(luot.Id, "Tai");
            luot.LanTruyCapCuoi = DateTime.Now;
            await Db.SaveChangesAsync();

            var tenTep = TenTepAnToan(tep.Sach.TieuDe) + "." + tep.DinhDang.ToLowerInvariant();
            return PhysicalFile(duongDan, LayMime(tep.DinhDang), tenTep, enableRangeProcessing: true);
        }

        // =====================================================
        // HÀM HỖ TRỢ
        // =====================================================

        // Kiểm tra mọi điều kiện để được mở tệp. Trả về quyền đọc (kèm tệp, sách) hoặc thông báo lỗi.
        private async Task<(LuotDocSachDienTu? Luot, string? Loi)> KiemTraQuyenAsync(int luotDocId, DocGium dg)
        {
            var bayGio = DateTime.Now;
            var homNay = DateOnly.FromDateTime(bayGio);

            var luot = await Db.LuotDocSachDienTus
                .Include(l => l.TepSach)
                    .ThenInclude(t => t.Sach)
                .FirstOrDefaultAsync(l => l.Id == luotDocId && l.DocGiaId == dg.Id);

            if (luot == null)
                return (null, "Không tìm thấy quyền đọc của bạn.");

            if (dg.TrangThai != "HoatDong" || dg.NgayHetHan < homNay)
                return (null, "Thẻ thư viện của bạn đã hết hạn hoặc đang bị khóa.");

            if (luot.TrangThai == "ThuHoi")
                return (null, "Quyền đọc này đã bị thu hồi.");

            if (luot.TrangThai != "HieuLuc" || luot.NgayHetHan <= bayGio)
                return (null, "Quyền đọc này đã hết hạn. Hãy đăng ký đọc lại ở trang chi tiết sách.");

            var tep = luot.TepSach;

            if (tep.TrangThai != "HoatDong" || tep.Sach.TrangThai != "HoatDong")
                return (null, "Tệp sách này hiện không khả dụng.");

            if (tep.NgayHetBanQuyen.HasValue && tep.NgayHetBanQuyen.Value < homNay)
                return (null, "Tệp sách này đã hết bản quyền phát hành.");

            return (luot, null);
        }

        // Đóng các quyền đọc đã quá hạn của độc giả (HieuLuc -> HetHan)
        private async Task HetHanQuyenAsync(int docGiaId)
        {
            var bayGio = DateTime.Now;

            var quaHan = await Db.LuotDocSachDienTus
                .Where(l => l.DocGiaId == docGiaId && l.TrangThai == "HieuLuc" && l.NgayHetHan <= bayGio)
                .ToListAsync();

            if (quaHan.Count == 0) return;

            foreach (var l in quaHan)
                l.TrangThai = "HetHan";

            await Db.SaveChangesAsync();
        }

        // Thêm một dòng nhật ký truy cập (người gọi chịu trách nhiệm SaveChanges)
        private void GhiNhatKy(int luotDocId, string hanhDong)
        {
            Db.NhatKyTruyCaps.Add(new NhatKyTruyCap
            {
                LuotDocId = luotDocId,
                ThoiDiem = DateTime.Now,
                HanhDong = hanhDong,
                DiaChiIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
        }

        // Token ngắn hạn: chứa mã quyền đọc và mã độc giả, tự hết hạn sau vài phút
        private string TaoToken(int luotDocId, int docGiaId) =>
            _protector.Protect($"{luotDocId}:{docGiaId}", TimeSpan.FromMinutes(SoPhutHieuLucToken));

        private bool GiaiMaToken(string token, out int luotDocId, out int docGiaId)
        {
            luotDocId = 0;
            docGiaId = 0;

            try
            {
                var phan = _protector.Unprotect(token).Split(':');
                return phan.Length == 2
                    && int.TryParse(phan[0], out luotDocId)
                    && int.TryParse(phan[1], out docGiaId);
            }
            catch (CryptographicException)
            {
                return false;   // sai token hoặc đã hết hạn
            }
        }

        // Tệp nằm ngoài wwwroot (thư mục App_Data), không truy cập được bằng URL trực tiếp.
        // Chặn đường dẫn thoát ra ngoài thư mục gốc.
        private string? LayDuongDanTep(string duongDanTrongDb)
        {

            var thuMucGoc = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "App_Data", "ebooks"));
            var duongDan = Path.GetFullPath(Path.Combine(thuMucGoc, duongDanTrongDb));

            if (!duongDan.StartsWith(thuMucGoc + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return null;

            return System.IO.File.Exists(duongDan) ? duongDan : null;
        }

        private static string LayMime(string dinhDang) => dinhDang.ToUpperInvariant() switch
        {
            "PDF" => "application/pdf",
            "EPUB" => "application/epub+zip",
            _ => "application/octet-stream"
        };

        private static string TenTepAnToan(string tieuDe)
        {
            var khongHopLe = Path.GetInvalidFileNameChars();
            var ten = new string(tieuDe.Where(c => !khongHopLe.Contains(c)).ToArray()).Trim();
            return ten.Length == 0 ? "sach-dien-tu" : ten;
        }
    }
}