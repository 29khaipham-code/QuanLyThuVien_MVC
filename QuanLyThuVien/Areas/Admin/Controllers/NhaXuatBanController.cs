
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.NhaXuatBan;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class NhaXuatBanController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        public NhaXuatBanController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        // ====================================
        // 1. DANH SÁCH + TÌM KIẾM + PHÂN TRANG
        // ====================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword,
            int page = 1)
        {
            const int pageSize = 5;

            if (page < 1)
                page = 1;

            keyword = keyword?.Trim();

            var query = _db.Set<NhaXuatBan>()
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x =>
                    x.TenNhaXuatBan.Contains(keyword) ||
                    (x.DiaChi != null &&
                     x.DiaChi.Contains(keyword)) ||
                    (x.Email != null &&
                     x.Email.Contains(keyword)));
            }

            int totalItems = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var danhSach = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(danhSach);
        }

        // ====================================
        // 2. HIỂN THỊ FORM THÊM
        // ====================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new NhaXuatBanFormViewModel());
        }

        // ====================================
        // 3. LƯU NHÀ XUẤT BẢN MỚI
        // ====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            NhaXuatBanFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string ten = model.TenNhaXuatBan.Trim();

            bool daTonTai = await _db.Set<NhaXuatBan>()
                .AnyAsync(x => x.TenNhaXuatBan == ten);

            if (daTonTai)
            {
                ModelState.AddModelError(
                    nameof(model.TenNhaXuatBan),
                    "Nhà xuất bản này đã tồn tại");

                return View(model);
            }

            var nxb = new NhaXuatBan
            {
                TenNhaXuatBan = ten,
                DiaChi = model.DiaChi?.Trim(),
                SoDienThoai = model.SoDienThoai?.Trim(),
                Email = model.Email?.Trim(),
                Website = model.Website?.Trim()
            };

            _db.Set<NhaXuatBan>().Add(nxb);
            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Thêm nhà xuất bản thành công!";

            return RedirectToAction(nameof(Index));
        }

        // ====================================
        // 4. HIỂN THỊ FORM SỬA
        // ====================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var nxb = await _db.Set<NhaXuatBan>()
                .FindAsync(id);

            if (nxb == null)
                return NotFound();

            var model = new NhaXuatBanFormViewModel
            {
                Id = nxb.Id,
                TenNhaXuatBan = nxb.TenNhaXuatBan,
                DiaChi = nxb.DiaChi,
                SoDienThoai = nxb.SoDienThoai,
                Email = nxb.Email,
                Website = nxb.Website
            };

            return View(model);
        }

        // ====================================
        // 5. LƯU THÔNG TIN CHỈNH SỬA
        // ====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            NhaXuatBanFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var nxb = await _db.Set<NhaXuatBan>()
                .FindAsync(id);

            if (nxb == null)
                return NotFound();

            string ten = model.TenNhaXuatBan.Trim();

            bool trungTen = await _db.Set<NhaXuatBan>()
                .AnyAsync(x =>
                    x.TenNhaXuatBan == ten &&
                    x.Id != id);

            if (trungTen)
            {
                ModelState.AddModelError(
                    nameof(model.TenNhaXuatBan),
                    "Tên nhà xuất bản đã tồn tại");

                return View(model);
            }

            nxb.TenNhaXuatBan = ten;
            nxb.DiaChi = model.DiaChi?.Trim();
            nxb.SoDienThoai = model.SoDienThoai?.Trim();
            nxb.Email = model.Email?.Trim();
            nxb.Website = model.Website?.Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật nhà xuất bản thành công!";

            return RedirectToAction(nameof(Index));
        }

        // ====================================
        // 6. XÓA NHÀ XUẤT BẢN
        // ====================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var nxb = await _db.Set<NhaXuatBan>()
                .FindAsync(id);

            if (nxb == null)
                return NotFound();

            // Không xóa nhà xuất bản đang có sách
            bool dangCoSach = await _db.Set<Sach>()
                .AnyAsync(x => x.NhaXuatBanId == id);

            if (dangCoSach)
            {
                TempData["Error"] =
                    "Không thể xóa vì nhà xuất bản đang có sách!";

                return RedirectToAction(nameof(Index));
            }

            _db.Set<NhaXuatBan>().Remove(nxb);

            try
            {
                await _db.SaveChangesAsync();

                TempData["Success"] =
                    "Xóa nhà xuất bản thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa nhà xuất bản vì dữ liệu đang được sử dụng!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
