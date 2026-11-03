using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Areas.DocGia.Helpers;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels;
using QuanLyThuVien.ViewModels.DocGia;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public class SachController : DocGiaControllerBase
    {
        private const int KichThuocTrang = 8;

        public SachController(QuanLyThuVienContext db) : base(db)
        {
        }

        // DANH SÁCH: lọc + sắp xếp + phân trang. Yêu cầu AJAX chỉ trả về phần danh sách.
        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa, int? theLoaiId, int? nhaXuatBanId,
            bool conSach = false, string? sapXep = "moi", int trang = 1)
        {
            tuKhoa = tuKhoa?.Trim();

            var query = Db.Saches.AsNoTracking()
                .Where(s => s.TrangThai == "HoatDong");

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(s =>
                    s.TieuDe.Contains(tuKhoa) ||
                    (s.Isbn != null && s.Isbn.Contains(tuKhoa)) ||
                    s.TacGia.Any(t => t.TenTacGia.Contains(tuKhoa)));
            }

            if (theLoaiId.HasValue)
                query = query.Where(s => s.TheLoaiId == theLoaiId.Value);

            if (nhaXuatBanId.HasValue)
                query = query.Where(s => s.NhaXuatBanId == nhaXuatBanId.Value);

            if (conSach)
                query = query.Where(s => s.BanSaoSaches.Any(b => b.TrangThai == "SanSang"));

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
                NhaXuatBanId = nhaXuatBanId,
                ConSach = conSach,
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

            vm.NhaXuatBans = await Db.NhaXuatBans.AsNoTracking()
                .OrderBy(n => n.TenNhaXuatBan)
                .Select(n => new LuaChonViewModel { Id = n.Id, Ten = n.TenNhaXuatBan })
                .ToListAsync();

            return View(vm);
        }

        // CHI TIẾT SÁCH (kèm trạng thái mượn / đặt trước của độc giả đang xem)
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var vm = await Db.Saches.AsNoTracking()
                .Where(s => s.Id == id && s.TrangThai == "HoatDong")
                .Select(s => new SachChiTietViewModel
                {
                    Id = s.Id,
                    Isbn = s.Isbn,
                    TieuDe = s.TieuDe,
                    AnhBiaUrl = s.AnhBiaUrl,
                    MoTa = s.MoTa,
                    TheLoai = s.TheLoai.TenTheLoai,
                    NhaXuatBan = s.NhaXuatBan.TenNhaXuatBan,
                    NamXuatBan = s.NamXuatBan,
                    LanTaiBan = s.LanTaiBan,
                    TacGias = s.TacGia.Select(t => t.TenTacGia).ToList(),
                    DinhDangs = s.TepSachDienTus
                        .Where(t => t.TrangThai == "HoatDong")
                        .Select(t => t.DinhDang)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (vm == null)
                return NotFound();

            // Số lượng bản sao lấy từ view vw_SoLuongSach
            var sl = await Db.VwSoLuongSaches.AsNoTracking()
                .FirstOrDefaultAsync(v => v.SachId == id);

            if (sl != null)
            {
                vm.TongBanSao = (sl.TongBanSao ?? 0) - (sl.DaThanhLy ?? 0);
                vm.SanSang = sl.SanSang ?? 0;
                vm.DangMuon = sl.DangMuon ?? 0;
                vm.DaDat = sl.DaDat ?? 0;
            }

            // Trạng thái của độc giả đối với cuốn sách này
            var dg = await LayDocGiaAsync();
            if (dg != null)
            {
                vm.CoHoSo = true;

                vm.DangMuonCuonNay = await Db.ChiTietMuons.AnyAsync(c =>
                    c.NgayTra == null
                    && c.PhieuMuon.DocGiaId == dg.Id
                    && c.PhieuMuon.TrangThai != "Huy"
                    && c.BanSao.SachId == id);

                var dat = await Db.DatTruocs.AsNoTracking()
                    .Where(d => d.DocGiaId == dg.Id && d.SachId == id
                             && (d.TrangThai == "DangCho" || d.TrangThai == "DaGiu"))
                    .FirstOrDefaultAsync();

                if (dat != null)
                {
                    vm.DatTruocTrangThai = dat.TrangThai;

                    if (dat.TrangThai == "DangCho")
                    {
                        vm.ViTriHangDoi = 1 + await Db.DatTruocs.CountAsync(d =>
                            d.SachId == id && d.TrangThai == "DangCho" && d.NgayDat < dat.NgayDat);
                    }
                }
            }

            return View(vm);
        }
    }
}
