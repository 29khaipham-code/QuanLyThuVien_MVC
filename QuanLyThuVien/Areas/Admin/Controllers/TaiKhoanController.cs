
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThuVien.ViewModels.TaiKhoan;

using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.TaiKhoan;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyThuVienContext _context;

        public TaiKhoanController(
            QuanLyThuVienContext context)
        {
            _context = context;
        }

        // Danh sách và tìm kiếm tài khoản
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword)
        {
            var query =
                from tk in _context.Set<QuanLyThuVien.Models.TaiKhoan>()
                join vt in _context.Set<VaiTro>()
                    on tk.VaiTroId equals vt.Id
                select new TaiKhoanListItem
                {
                    Id = tk.Id,
                    TenDangNhap = tk.TenDangNhap,
                    HoTen = tk.HoTen,
                    Email = tk.Email,
                    TenVaiTro = vt.TenVaiTro,
                    HoatDong = tk.HoatDong,
                    NgayTao = tk.NgayTao
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(x =>
                    x.TenDangNhap.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    x.Email.Contains(keyword));
            }

            var danhSach = await query
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            ViewBag.Keyword = keyword;

            return View(danhSach);
        }

        // Khóa hoặc mở khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            var taiKhoan = await _context
                .Set<QuanLyThuVien.Models.TaiKhoan>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (taiKhoan == null)
                return NotFound();

            // Không cho Admin tự khóa tài khoản đang dùng.
            // Login cần lưu claim NameIdentifier = TaiKhoan.Id.
            var currentId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            if (currentId == taiKhoan.Id.ToString())
            {
                TempData["Error"] =
                    "Không thể tự khóa tài khoản của mình.";

                return RedirectToAction(nameof(Index));
            }

            taiKhoan.HoatDong = !taiKhoan.HoatDong;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật trạng thái tài khoản thành công.";

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị form thêm tài khoản
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadVaiTro();
            return View(new TaoTaiKhoanViewModel());
        }

        // Lưu danh sách vai trò vào dropdown
        private async Task LoadVaiTro()
        {
            var vaiTros = await _context.Set<VaiTro>()
                .Where(x => x.TenVaiTro == "ThuThu"
                         || x.TenVaiTro == "DocGia")
                .OrderBy(x => x.TenVaiTro)
                .ToListAsync();

            ViewBag.VaiTro = new SelectList(
                vaiTros,
                "Id",
                "TenVaiTro"
            );
        }

    }
}
