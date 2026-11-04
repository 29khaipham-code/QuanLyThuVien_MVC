using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Areas.DocGia.Helpers;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.TrangChu;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    // Khách (chưa đăng nhập) cũng xem được trang chủ; phần cá nhân chỉ hiện với độc giả đã đăng nhập
    [Area("DocGia")]
    [AllowAnonymous]
    public class HomeController : DocGiaControllerBase
    {
        private const int SoSachMoiKe = 8;
        private const int SoKeChuDe = 3;

        public HomeController(QuanLyThuVienContext db) : base(db)
        {
        }

        public async Task<IActionResult> Index()
        {
            var vm = new TrangChuViewModel
            {
                LaDocGia = LaDocGia,
                Banners = TaoBanner(LaDocGia)
            };

            var sachHoatDong = Db.Saches.AsNoTracking().Where(s => s.TrangThai == "HoatDong");

            // 1. Hồ sơ độc giả (null nếu là khách, nhân viên hoặc chưa có hồ sơ)
            var dg = await LayDocGiaAsync();

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
            //    Khách hoặc chưa có lịch sử thì gợi ý các sách được mượn nhiều nhất.
            var theLoaiYeuThich = tiepCan.Count == 0
                ? new List<int>()
                : await Db.Saches.AsNoTracking()
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
                    .OrderByDescending(s => Db.ChiTietMuons.Count(c => c.BanSao.SachId == s.Id))
                    .ThenByDescending(s => s.Id)
                    .Take(SoSachMoiKe)
                    .ToItemViewModel()
                    .ToListAsync()
            });

            // 4. Kệ "Sách đã đọc" (chỉ khi là độc giả có lịch sử)
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
                MoTa = LaDocGia ? "Đọc trực tuyến mọi lúc mọi nơi" : "Đăng nhập để đọc trực tuyến",
                Icon = "bi-tablet",
                Items = await sachHoatDong
                    .Where(s => s.TepSachDienTus.Any(t => t.TrangThai == "HoatDong"))
                    .OrderByDescending(s => s.Id)
                    .Take(SoSachMoiKe)
                    .ToItemViewModel()
                    .ToListAsync()
            });

            // 6. Theo chủ đề: chip cho mọi thể loại + kệ cho các thể loại nhiều sách nhất
            vm.TheLoais = await Db.TheLoais.AsNoTracking()
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

        // Các slide của carousel. Độc giả: slide 3 mời mượn sách; khách: slide 3 mời đăng nhập.
        private List<BannerViewModel> TaoBanner(bool laDocGia)
        {
            var banners = new List<BannerViewModel>
            {
                new()
                {
                    TieuDe = "Chào mừng đến với thư viện",
                    MoTa = "Tra cứu sách giấy và sách điện tử ngay trên website.",
                    NutChu = "Tra cứu sách",
                    Url = Url.Action("Index", "Sach", new { area = "DocGia" })!,
                    LopNen = "bg-primary",
                    AnhUrl="images/Banners/banner-1.jpg"
                },
                new()
                {
                    TieuDe = "Sách mới về",
                    MoTa = "Cập nhật những đầu sách mới nhất của thư viện.",
                    NutChu = "Xem sách mới",
                    Url = Url.Action("Index", "Sach", new { area = "DocGia", sapXep = "moi" })!,
                    LopNen = "bg-success",
                    AnhUrl="images/Banners/banner-2.jpg"
                }
            };

            banners.Add(laDocGia
                ? new BannerViewModel
                {
                    TieuDe = "Mượn ngay hôm nay",
                    MoTa = "Lọc nhanh những cuốn còn sẵn trên kệ để đến mượn.",
                    NutChu = "Xem sách còn bản",
                    Url = Url.Action("Index", "Sach", new { area = "DocGia", conSach = "true" })!,
                    LopNen = "bg-dark",
                    AnhUrl = "images/Banners/banner-3.jpg"
                }
                : new BannerViewModel
                {
                    TieuDe = "Đăng nhập để dùng đầy đủ tính năng",
                    MoTa = "Đặt trước sách, theo dõi mượn trả và đọc sách điện tử chỉ với một tài khoản độc giả.",
                    NutChu = "Đăng nhập",
                    Url = Url.Action("Login", "Account", new { area = "" })!,
                    LopNen = "bg-dark",
                    AnhUrl = "images/Banners/banner-3.jpg"

                });

            return banners;
        }

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
            var dangMuon = Db.ChiTietMuons.AsNoTracking()
                .Where(c => c.PhieuMuon.DocGiaId == dg.Id
                         && c.PhieuMuon.TrangThai != "Huy"
                         && c.NgayTra == null);

            var homNay = DateOnly.FromDateTime(DateTime.Today);

            vm.DangMuon = await dangMuon.CountAsync();
            vm.QuaHan = await dangMuon.CountAsync(c => c.HanTra < homNay);
            vm.SapDenHan = await dangMuon.CountAsync(c =>
                c.HanTra >= homNay && c.HanTra <= homNay.AddDays(3));

            vm.PhatChuaThanhToan = await Db.Phats.AsNoTracking()
                .Where(p => p.ChiTietMuon.PhieuMuon.DocGiaId == dg.Id && !p.DaThanhToan)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;
        }

        // Lịch sử mượn/đọc của độc giả
        private async Task<(List<int> TiepCan, List<int> DaDoc)> LayLichSu(int docGiaId)
        {
            var muon = await Db.ChiTietMuons.AsNoTracking()
                .Where(c => c.PhieuMuon.DocGiaId == docGiaId && c.PhieuMuon.TrangThai != "Huy")
                .Select(c => new { SachId = c.BanSao.SachId, Ngay = c.NgayTra })
                .ToListAsync();

            var doc = await Db.LuotDocSachDienTus.AsNoTracking()
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
