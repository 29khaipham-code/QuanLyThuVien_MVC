
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuanLyThuVien.ViewModels.Admin.TaiKhoan;

using QuanLyThuVien.Models;


namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class TaiKhoanController : Controller
    {
        private readonly QuanLyThuVienContext _context;
        private readonly IPasswordHasher<QuanLyThuVien.Models.TaiKhoan> _passwordHasher;

        public TaiKhoanController(
            QuanLyThuVienContext context,
            IPasswordHasher<QuanLyThuVien.Models.TaiKhoan> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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

        // Tạo tài khoản mới (chỉ thêm, không chỉnh sửa tài khoản hiện có)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoTaiKhoanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadVaiTro();
                return View(model);
            }

            model.TenDangNhap = model.TenDangNhap.Trim();
            model.HoTen = model.HoTen.Trim();
            model.Email = model.Email.Trim();
            model.SoDienThoai = model.SoDienThoai?.Trim();

            if (await _context.Set<QuanLyThuVien.Models.TaiKhoan>()
                .AnyAsync(x => x.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError(nameof(model.TenDangNhap),
                    "Tên đăng nhập đã tồn tại.");
            }

            if (await _context.Set<QuanLyThuVien.Models.TaiKhoan>()
                .AnyAsync(x => x.Email == model.Email))
            {
                ModelState.AddModelError(nameof(model.Email),
                    "Email đã được sử dụng.");
            }

            // Chỉ cho phép các vai trò được hiển thị trong form; không thể tự tạo Admin.
            var vaiTro = await _context.Set<VaiTro>()
                .FirstOrDefaultAsync(x =>
                    x.Id == model.VaiTroId &&
                    (x.TenVaiTro == "ThuThu" || x.TenVaiTro == "DocGia"));

            if (vaiTro == null)
            {
                ModelState.AddModelError(nameof(model.VaiTroId),
                    "Vai trò không hợp lệ.");
            }

            LoaiDocGium? loaiDocGia = null;
            if (vaiTro?.TenVaiTro == "DocGia")
            {
                loaiDocGia = await _context.LoaiDocGia
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (loaiDocGia == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "Chưa có loại độc giả trong hệ thống. Vui lòng tạo loại độc giả trước.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadVaiTro();
                return View(model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var taiKhoan = new QuanLyThuVien.Models.TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                        ? null : model.SoDienThoai,
                    HoatDong = true,
                    NgayTao = DateTime.Now,
                    VaiTroId = vaiTro!.Id
                };

                // Dùng cùng PasswordHasher với chức năng đăng nhập hiện tại.
                taiKhoan.MatKhauHash =
                    _passwordHasher.HashPassword(taiKhoan, model.MatKhau);

                _context.Set<QuanLyThuVien.Models.TaiKhoan>().Add(taiKhoan);
                await _context.SaveChangesAsync();

                // Tài khoản Độc giả phải có hồ sơ DocGia để dùng các chức năng mượn/trả.
                if (vaiTro.TenVaiTro == "DocGia")
                {
                    var homNay = DateOnly.FromDateTime(DateTime.Today);
                    _context.DocGia.Add(new DocGium
                    {
                        MaDocGia = "DG" + Guid.NewGuid().ToString("N")[..18],
                        HoTen = model.HoTen,
                        Email = model.Email,
                        SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                            ? null : model.SoDienThoai,
                        NgayDangKy = homNay,
                        NgayHetHan = homNay.AddYears(1),
                        TrangThai = "HoatDong",
                        TaiKhoanId = taiKhoan.Id,
                        LoaiDocGiaId = loaiDocGia!.Id
                    });
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = "Thêm tài khoản mới thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    "Không thể tạo tài khoản. Tên đăng nhập hoặc email có thể đã tồn tại.");
                await LoadVaiTro();
                return View(model);
            }
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

        [HttpGet]
        public async Task<IActionResult> GoiY(string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Array.Empty<object>());

            term = term.Trim();

            var data = await _context.Set<QuanLyThuVien.Models.TaiKhoan>()
                .AsNoTracking()
                .Where(x =>
                    x.TenDangNhap.Contains(term) ||
                    x.HoTen.Contains(term) ||
                    x.Email.Contains(term))
                .OrderBy(x => x.HoTen.StartsWith(term) ? 0 : 1)
                .ThenBy(x => x.HoTen)
                .Select(x => new
                {
                    value = x.TenDangNhap,
                    label = x.HoTen,
                    detail = x.TenDangNhap + " | " + x.Email
                })
                .Take(10)
                .ToListAsync();

            return Json(data);
        }


    }
}
