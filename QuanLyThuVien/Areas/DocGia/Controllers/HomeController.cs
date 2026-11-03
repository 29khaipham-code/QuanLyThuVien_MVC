using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Areas.DocGia.Helpers;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.TrangChu;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public class HomeController : Controller
    {
        private const int SoSachMoiKe = 8;
        private const int SoKeChuDe = 3;

        private readonly QuanLyThuVienContext _db;

        public HomeController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new TrangChuViewModel { Banners = TaoBanner() };

            var sachHoatDong = _db.Saches.AsNoTracking().Where(s => s.TrangThai == "HoatDong");

            // 1. Hồ sơ độc giả gắn với tài khoản đang đăng nhập (có thể chưa có)
            DocGium? dg = null;
            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId))
            {
                dg = await _db.DocGia.AsNoTracking()
                    .Include(d => d.LoaiDocGia)
                    .FirstOrDefaultAsync(d => d.TaiKhoanId == taiKhoanId);
            }

            // Lịch sử: tiepCan = mọi sách từng mượn/đọc, daDocIds = sách đã trả hoặc đã mở đọc (mới nhất trước)
            var tiepCan = new List<int>();
            var daDocIds = new List<int>();

            if (dg != null)
            {
                await NapThongTinDocGia(vm, dg);
                (tiepCan, daDocIds) = await LayLichSu(dg.Id);
            }

            // 2. Kệ "Sách mới nhất"
            vm.KeSach.Add(new KeSachViewModel
            {
                TieuDe = "Sách mới nhất",
                MoTa = "Vừa được bổ sung vào thư viện",
                Icon = "bi-stars",
                Url = Url.Action("Index", "Sach", new { area = "DocGia", sapXep = "moi" }),
                Items = await sachHoatDong
                    .OrderByDescending(s => s.Id)
                    .Take(SoSachMoiKe)
                    .ToItemViewModel()
                    .ToListAsync()
            });

            // 3. Kệ "Gợi ý cho bạn": cùng thể loại với sách đã mượn/đọc, chưa mượn, nhiều lượt mượn nhất.
            //    Chưa có lịch sử thì gợi ý các sách được mượn nhiều nhất.
            var theLoaiYeuThich = tiepCan.Count == 0
                ? new List<int>()
                : await _db.Saches.AsNoTracking()
                    .Where(s => tiepCan.Contains(s.Id))
                    .GroupBy(s => s.TheLoaiId)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(3)
                    .ToListAsync();

            var goiY = sachHoatDong.Where(s => !tiepCan.Contains(s.Id));
            if (theLoaiYeuThich.Count > 0)
                goiY = goiY.Where(s => theLoaiYeuThich.Contains(s.TheLoaiId));

            vm.KeSach.Add(new KeSachViewModel
            {
                TieuDe = theLoaiYeuThich.Count > 0 ? "Gợi ý cho bạn" : "Được quan tâm nhiều",
                MoTa = theLoaiYeuThich.Count > 0
                    ? "Dựa trên thể loại bạn đã mượn và đọc"
                    : "Những cuốn được mượn nhiều nhất",
                Icon = "bi-lightbulb",
                Items = await goiY
                    .OrderByDescending(s => _db.ChiTietMuons.Count(c => c.BanSao.SachId == s.Id))
                    .ThenByDescending(s => s.Id)
                    .Take(SoSachMoiKe)
                    .ToItemViewModel()
                    .ToListAsync()
            });

            // 4. Kệ "Sách đã đọc" (chỉ khi có hồ sơ và có lịch sử)
            if (daDocIds.Count > 0)
            {
                var daDoc = await sachHoatDong
                    .Where(s => daDocIds.Contains(s.Id))
                    .ToItemViewModel()
                    .ToListAsync();

                vm.KeSach.Add(new KeSachViewModel
                {
                    TieuDe = "Sách bạn đã đọc",
                    MoTa = "Những cuốn bạn đã trả hoặc đã mở đọc gần đây",
                    Icon = "bi-check2-circle",
                    Items = daDoc.OrderBy(s => daDocIds.IndexOf(s.Id)).ToList()
                });
            }

            // 5. Kệ "Sách điện tử"
            vm.KeSach.Add(new KeSachViewModel
            {
                TieuDe = "Sách điện tử",
                MoTa = "Đọc trực tuyến mọi lúc mọi nơi",
                Icon = "bi-tablet",
                Items = await sachHoatDong
                    .Where(s => s.TepSachDienTus.Any(t => t.TrangThai == "HoatDong"))
                    .OrderByDescending(s => s.Id)
                    .Take(SoSachMoiKe)
                    .ToItemViewModel()
                    .ToListAsync()
            });

            // 6. Theo chủ đề: chip cho mọi thể loại + kệ cho các thể loại nhiều sách nhất
            vm.TheLoais = await _db.TheLoais.AsNoTracking()
                .Select(t => new TheLoaiThongKeViewModel
                {
                    Id = t.Id,
                    Ten = t.TenTheLoai,
                    SoSach = t.Saches.Count(s => s.TrangThai == "HoatDong")
                })
                .Where(t => t.SoSach > 0)
                .OrderBy(t => t.Ten)
                .ToListAsync();

            foreach (var t in vm.TheLoais.OrderByDescending(t => t.SoSach).Take(SoKeChuDe))
            {
                vm.KeTheoChuDe.Add(new KeSachViewModel
                {
                    TieuDe = t.Ten,
                    Icon = "bi-bookmark",
                    Url = Url.Action("Index", "Sach", new { area = "DocGia", theLoaiId = t.Id }),
                    Items = await sachHoatDong
                        .Where(s => s.TheLoaiId == t.Id)
                        .OrderByDescending(s => s.Id)
                        .Take(SoSachMoiKe)
                        .ToItemViewModel()
                        .ToListAsync()
                });
            }

            return View(vm);
        }

        // ---------- Hàm hỗ trợ ----------

        // Các slide của carousel (muốn dùng ảnh thì gán AnhUrl, ví dụ "images/banner/b1.jpg")
        private List<BannerViewModel> TaoBanner() => new()
        {
            new BannerViewModel
            {
                TieuDe = "Chào mừng đến với thư viện",
                MoTa = "Tra cứu sách giấy và sách điện tử, đặt trước và theo dõi việc mượn trả ngay trên website.",
                NutChu = "Tra cứu sách",
                Url = Url.Action("Index", "Sach", new { area = "DocGia" })!,
                LopNen = "bg-primary"
            },
            new BannerViewModel
            {
                TieuDe = "Sách mới về",
                MoTa = "Cập nhật những đầu sách mới nhất của thư viện.",
                NutChu = "Xem sách mới",
                Url = Url.Action("Index", "Sach", new { area = "DocGia", sapXep = "moi" })!,
                LopNen = "bg-success"
            },
            new BannerViewModel
            {
                TieuDe = "Mượn ngay hôm nay",
                MoTa = "Lọc nhanh những cuốn còn sẵn trên kệ để đến mượn.",
                NutChu = "Xem sách còn bản",
                Url = Url.Action("Index", "Sach", new { area = "DocGia", conSach = "true" })!,
                LopNen = "bg-dark"
            }
        };

        private async Task NapThongTinDocGia(TrangChuViewModel vm, DocGium dg)
        {
            vm.CoHoSo = true;
            vm.HoTen = dg.HoTen;
            vm.MaDocGia = dg.MaDocGia;
            vm.LoaiDocGia = dg.LoaiDocGia.TenLoai;
            vm.NgayHetHan = dg.NgayHetHan;
            vm.TrangThai = dg.TrangThai;
            vm.SoSachToiDa = dg.LoaiDocGia.SoSachToiDa;

            // Sách đang mượn = chi tiết mượn chưa có ngày trả, thuộc phiếu chưa hủy
            var dangMuon = _db.ChiTietMuons.AsNoTracking()
                .Where(c => c.PhieuMuon.DocGiaId == dg.Id
                         && c.PhieuMuon.TrangThai != "Huy"
                         && c.NgayTra == null);

            var homNay = DateOnly.FromDateTime(DateTime.Today);

            vm.DangMuon = await dangMuon.CountAsync();
            vm.QuaHan = await dangMuon.CountAsync(c => c.HanTra < homNay);
            vm.SapDenHan = await dangMuon.CountAsync(c =>
                c.HanTra >= homNay && c.HanTra <= homNay.AddDays(3));

            vm.PhatChuaThanhToan = await _db.Phats.AsNoTracking()
                .Where(p => p.ChiTietMuon.PhieuMuon.DocGiaId == dg.Id && !p.DaThanhToan)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;
        }

        // Lịch sử mượn/đọc của độc giả
        private async Task<(List<int> TiepCan, List<int> DaDoc)> LayLichSu(int docGiaId)
        {
            var muon = await _db.ChiTietMuons.AsNoTracking()
                .Where(c => c.PhieuMuon.DocGiaId == docGiaId && c.PhieuMuon.TrangThai != "Huy")
                .Select(c => new { SachId = c.BanSao.SachId, Ngay = c.NgayTra })
                .ToListAsync();

            var doc = await _db.LuotDocSachDienTus.AsNoTracking()
                .Where(l => l.DocGiaId == docGiaId)
                .Select(l => new { SachId = l.TepSach.SachId, Ngay = (DateTime?)(l.LanTruyCapCuoi ?? l.NgayBatDau) })
                .ToListAsync();

            var tiepCan = muon.Select(x => x.SachId)
                .Union(doc.Select(x => x.SachId))
                .ToList();

            // Đã đọc: đã trả sách (có ngày trả) hoặc đã mở sách điện tử, mới nhất trước
            var daDoc = muon.Where(x => x.Ngay != null)
                .Concat(doc)
                .GroupBy(x => x.SachId)
                .OrderByDescending(g => g.Max(x => x.Ngay))
                .Select(g => g.Key)
                .Take(SoSachMoiKe)
                .ToList();

            return (tiepCan, daDoc);
        }
    }
}
