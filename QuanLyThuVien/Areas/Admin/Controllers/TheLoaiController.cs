
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.TheLoai;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class TheLoaiController : Controller
    {
        private readonly QuanLyThuVienContext _context;

        public TheLoaiController(QuanLyThuVienContext context)
        {
            _context = context;
        }

        // DANH SÁCH + TÌM KIẾM + PHÂN TRANG
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword, int page = 1)
        {
            const int pageSize = 5;
            page = Math.Max(1, page);

            var query = _context.Set<TheLoai>()
                .AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(x =>
                    x.TenTheLoai.Contains(keyword));
            }

            int count = await query.CountAsync();
            int totalPages = Math.Max(1,
                (int)Math.Ceiling(count / (double)pageSize));

            page = Math.Min(page, totalPages);

            var items = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }

        // FORM THÊM
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TheLoaiFormViewModel());
        }

        // LƯU THÊM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TheLoaiFormViewModel model)
        {
            model.TenTheLoai = model.TenTheLoai?.Trim() ?? "";
            model.MoTa = model.MoTa?.Trim();

            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Set<TheLoai>().AnyAsync(
                x => x.TenTheLoai == model.TenTheLoai))
            {
                ModelState.AddModelError(
                    nameof(model.TenTheLoai),
                    "Tên thể loại đã tồn tại.");
                return View(model);
            }

            _context.Set<TheLoai>().Add(new TheLoai
            {
                TenTheLoai = model.TenTheLoai,
                MoTa = model.MoTa
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("",
                    "Không thể lưu, tên thể loại có thể bị trùng.");
                return View(model);
            }

            TempData["Success"] = "Thêm thể loại thành công!";
            return RedirectToAction(nameof(Index));
        }

        // FORM SỬA
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Set<TheLoai>().FindAsync(id);
            if (item == null) return NotFound();

            return View(new TheLoaiFormViewModel
            {
                Id = item.Id,
                TenTheLoai = item.TenTheLoai,
                MoTa = item.MoTa
            });
        }

        // LƯU SỬA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, TheLoaiFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            model.TenTheLoai = model.TenTheLoai?.Trim() ?? "";
            model.MoTa = model.MoTa?.Trim();

            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
                return View(model);

            var item = await _context.Set<TheLoai>().FindAsync(id);
            if (item == null) return NotFound();

            if (await _context.Set<TheLoai>().AnyAsync(
                x => x.Id != id &&
                     x.TenTheLoai == model.TenTheLoai))
            {
                ModelState.AddModelError(
                    nameof(model.TenTheLoai),
                    "Tên thể loại đã tồn tại.");
                return View(model);
            }

            item.TenTheLoai = model.TenTheLoai;
            item.MoTa = model.MoTa;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("",
                    "Không thể cập nhật thể loại.");
                return View(model);
            }

            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }

        // XÓA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Set<TheLoai>().FindAsync(id);
            if (item == null) return NotFound();

            bool hasBooks = await _context.Set<Sach>()
                .AnyAsync(x => x.TheLoaiId == id);

            if (hasBooks)
            {
                TempData["Error"] =
                    "Không thể xóa thể loại đang có sách.";
                return RedirectToAction(nameof(Index));
            }

            _context.Set<TheLoai>().Remove(item);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa do dữ liệu liên quan.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
