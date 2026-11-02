
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.TacGia;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class TacGiaController : Controller
    {
        private readonly QuanLyThuVienContext _context;

        public TacGiaController(QuanLyThuVienContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH + TÌM KIẾM + PHÂN TRANG
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword, int page = 1)
        {
            const int pageSize = 5;
            page = Math.Max(1, page);

            IQueryable<TacGium> query = _context
                .Set<TacGium>()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>
                    x.TenTacGia.Contains(keyword));
            }

            int totalItems = await query.CountAsync();

            int totalPages = Math.Max(
                1,
                (int)Math.Ceiling(totalItems / (double)pageSize)
            );

            page = Math.Min(page, totalPages);

            var danhSach = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword ?? "";
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(danhSach);
        }

        // 2. HIỂN THỊ FORM THÊM
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TacGiaFormViewModel());
        }

        // 3. XỬ LÝ THÊM TÁC GIẢ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TacGiaFormViewModel model)
        {
            model.TenTacGia = model.TenTacGia?.Trim() ?? "";
            model.QuocTich = model.QuocTich?.Trim();
            model.TieuSu = model.TieuSu?.Trim();

            if (string.IsNullOrWhiteSpace(model.TenTacGia))
            {
                ModelState.AddModelError(
                    nameof(model.TenTacGia),
                    "Tên tác giả không được để trống");
            }

            if (!ModelState.IsValid)
                return View(model);

            var tacGia = new TacGium
            {
                TenTacGia = model.TenTacGia,
                TieuSu = model.TieuSu,
                NgaySinh = model.NgaySinh,
                QuocTich = model.QuocTich
            };

            _context.Set<TacGium>().Add(tacGia);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể thêm tác giả. Vui lòng thử lại.");

                return View(model);
            }

            TempData["Success"] = "Thêm tác giả thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 4. HIỂN THỊ FORM SỬA
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tacGia = await _context
                .Set<TacGium>()
                .FindAsync(id);

            if (tacGia == null)
                return NotFound();

            var model = new TacGiaFormViewModel
            {
                Id = tacGia.Id,
                TenTacGia = tacGia.TenTacGia,
                TieuSu = tacGia.TieuSu,
                NgaySinh = tacGia.NgaySinh,
                QuocTich = tacGia.QuocTich
            };

            return View(model);
        }

        // 5. XỬ LÝ SỬA TÁC GIẢ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, TacGiaFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            model.TenTacGia = model.TenTacGia?.Trim() ?? "";
            model.QuocTich = model.QuocTich?.Trim();
            model.TieuSu = model.TieuSu?.Trim();

            if (string.IsNullOrWhiteSpace(model.TenTacGia))
            {
                ModelState.AddModelError(
                    nameof(model.TenTacGia),
                    "Tên tác giả không được để trống");
            }

            if (!ModelState.IsValid)
                return View(model);

            var tacGia = await _context
                .Set<TacGium>()
                .FindAsync(id);

            if (tacGia == null)
                return NotFound();

            tacGia.TenTacGia = model.TenTacGia;
            tacGia.TieuSu = model.TieuSu;
            tacGia.NgaySinh = model.NgaySinh;
            tacGia.QuocTich = model.QuocTich;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật tác giả.");

                return View(model);
            }

            TempData["Success"] = "Cập nhật tác giả thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 6. XÓA TÁC GIẢ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tacGia = await _context
                .Set<TacGium>()
                .FindAsync(id);

            if (tacGia == null)
                return NotFound();

            // Không xóa tác giả đã có sách liên kết

            bool coSach = await _context.Set<TacGium>()
                .AnyAsync(x => x.Id == id && x.Saches.Any());


            if (coSach)
            {
                TempData["Error"] =
                    "Không thể xóa tác giả vì đã liên kết với sách.";

                return RedirectToAction(nameof(Index));
            }

            _context.Set<TacGium>().Remove(tacGia);

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Xóa tác giả thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa vì có dữ liệu liên quan.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
