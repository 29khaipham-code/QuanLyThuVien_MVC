
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.Admin.BanSaoSach;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class BanSaoSachController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        private static readonly string[] TrangThaiChoPhep =
        {
            "SanSang", "HuHong", "DangSua"
        };

        private static readonly string[] TinhTrangChoPhep =
        {
            "Moi", "Tot", "Cu", "HuHong"
        };

        public BanSaoSachController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        // 1. Danh sách + tìm kiếm + phân trang
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword, int page = 1)
        {
            const int pageSize = 10;
            page = Math.Max(page, 1);
            keyword = keyword?.Trim();

            var query =
                from bs in _db.Set<Models.BanSaoSach>()
                    .AsNoTracking()
                join s in _db.Set<Sach>().AsNoTracking()
                    on bs.SachId equals s.Id
                select new BanSaoSachListItemViewModel
                {
                    Id = bs.Id,
                    SachId = bs.SachId,
                    TenSach = s.TieuDe,
                    MaVach = bs.MaVach,
                    TrangThai = bs.TrangThai,
                    TinhTrang = bs.TinhTrang,
                    NgayNhap = bs.NgayNhap,
                    ViTri = bs.ViTri,
                    GiaTri = bs.GiaTri
                };

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    x.MaVach.Contains(keyword) ||
                    x.TenSach.Contains(keyword));
            }

            int total = await query.CountAsync();
            int totalPages =
                (int)Math.Ceiling(total / (double)pageSize);

            if (totalPages > 0)
                page = Math.Min(page, totalPages);

            var data = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            return View(data);
        }

        // Danh sách sách cho ComboBox
        private async Task LoadOptions()
        {
            var sach = await _db.Set<Sach>()
                .AsNoTracking()
                .OrderBy(x => x.TieuDe)
                .Select(x => new
                {
                    x.Id,
                    x.TieuDe
                })
                .ToListAsync();

            ViewBag.SachOptions = new SelectList(
                sach, "Id", "TieuDe");
        }

        // Kiểm tra dữ liệu nhập
        private async Task ValidateForm(
            BanSaoSachFormViewModel model,
            int? excludeId = null)
        {
            model.MaVach = model.MaVach?.Trim() ?? "";
            model.ViTri = model.ViTri?.Trim();

            bool sachTonTai = await _db.Set<Sach>()
                .AnyAsync(x => x.Id == model.SachId);

            if (!sachTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.SachId),
                    "Đầu sách không tồn tại.");
            }

            bool trungMaVach =
                await _db.Set<Models.BanSaoSach>()
                    .AnyAsync(x =>
                        x.MaVach == model.MaVach &&
                        (!excludeId.HasValue ||
                         x.Id != excludeId.Value));

            if (trungMaVach)
            {
                ModelState.AddModelError(
                    nameof(model.MaVach),
                    "Mã vạch đã tồn tại.");
            }

            if (!TrangThaiChoPhep.Contains(model.TrangThai))
            {
                ModelState.AddModelError(
                    nameof(model.TrangThai),
                    "Trạng thái không hợp lệ.");
            }

            if (!TinhTrangChoPhep.Contains(model.TinhTrang))
            {
                ModelState.AddModelError(
                    nameof(model.TinhTrang),
                    "Tình trạng không hợp lệ.");
            }
        }

        // 2. Form thêm
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOptions();

            return View(new BanSaoSachFormViewModel
            {
                NgayNhap = DateOnly.FromDateTime(
                    DateTime.Today)
            });
        }

        // 3. Lưu bản sao mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            BanSaoSachFormViewModel model)
        {
            await ValidateForm(model);

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            var banSao = new Models.BanSaoSach
            {
                SachId = model.SachId,
                MaVach = model.MaVach,
                TrangThai = model.TrangThai,
                TinhTrang = model.TinhTrang,
                NgayNhap = model.NgayNhap,
                ViTri = model.ViTri,
                GiaTri = model.GiaTri
            };

            _db.Set<Models.BanSaoSach>().Add(banSao);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "", "Không lưu được bản sao. " +
                    "Hãy kiểm tra mã vạch và dữ liệu nhập.");

                await LoadOptions();
                return View(model);
            }

            TempData["Success"] =
                "Thêm bản sao sách thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 4. Form sửa
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var bs = await _db.Set<Models.BanSaoSach>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bs == null)
                return NotFound();

            var model = new BanSaoSachFormViewModel
            {
                Id = bs.Id,
                SachId = bs.SachId,
                MaVach = bs.MaVach,
                TrangThai = bs.TrangThai,
                TinhTrang = bs.TinhTrang,
                NgayNhap = bs.NgayNhap,
                ViTri = bs.ViTri,
                GiaTri = bs.GiaTri
            };

            await LoadOptions();
            return View(model);
        }

        // 5. Lưu thay đổi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, BanSaoSachFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            var bs = await _db.Set<Models.BanSaoSach>()
                .FindAsync(id);

            if (bs == null)
                return NotFound();

            // Không cho sửa bản sao đang có nghiệp vụ
            if (!TrangThaiChoPhep.Contains(bs.TrangThai))
            {
                ModelState.AddModelError(
                    "",
                    "Bản sao đang mượn, đặt trước, " +
                    "đã mất hoặc thanh lý. " +
                    "Không thể sửa tại màn hình này.");

                await LoadOptions();
                return View(model);
            }

            await ValidateForm(model, id);

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            bs.SachId = model.SachId;
            bs.MaVach = model.MaVach;
            bs.TrangThai = model.TrangThai;
            bs.TinhTrang = model.TinhTrang;
            bs.NgayNhap = model.NgayNhap;
            bs.ViTri = model.ViTri;
            bs.GiaTri = model.GiaTri;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật bản sao. " +
                    "Kiểm tra mã vạch và dữ liệu liên quan.");

                await LoadOptions();
                return View(model);
            }

            TempData["Success"] =
                "Cập nhật bản sao sách thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 6. Xóa bản sao chưa từng có giao dịch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var bs = await _db.Set<Models.BanSaoSach>()
                .FindAsync(id);

            if (bs == null)
                return NotFound();

            if (!TrangThaiChoPhep.Contains(bs.TrangThai))
            {
                TempData["Error"] =
                    "Không thể xóa bản sao đang có nghiệp vụ.";
                return RedirectToAction(nameof(Index));
            }

            // Không xóa bản sao đã xuất hiện trong lịch sử mượn
            bool daCoMuon = await _db.Set<ChiTietMuon>()
                .AnyAsync(x => x.BanSaoId == id);

            if (daCoMuon)
            {
                TempData["Error"] =
                    "Không thể xóa bản sao đã có lịch sử mượn trả.";
                return RedirectToAction(nameof(Index));
            }

            _db.Set<Models.BanSaoSach>().Remove(bs);

            try
            {
                await _db.SaveChangesAsync();

                TempData["Success"] =
                    "Xóa bản sao sách thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa vì bản sao đang được " +
                    "tham chiếu bởi dữ liệu khác.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
