using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels;

namespace QuanLyThuVien.Controllers
{
    public class AccountController : Controller
    {
        private const string LoiDangNhap = "Tài khoản hoặc mật khẩu không đúng";

        private readonly QuanLyThuVienContext _db;
        private readonly IPasswordHasher<TaiKhoan> _hasher;

        public AccountController(
            QuanLyThuVienContext db,
            IPasswordHasher<TaiKhoan> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // ĐĂNG NHẬP (GET)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // Đã đăng nhập thì đưa thẳng về khu vực theo vai trò
            if (User.Identity?.IsAuthenticated == true)
                return ChuyenHuong(User.FindFirstValue(ClaimTypes.Role), returnUrl);

            ViewData["ReturnUrl"] = returnUrl;
            return View(new DangNhapViewModel());
        }

        // ĐĂNG NHẬP (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            DangNhapViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var tk = await _db.TaiKhoans
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x => x.TenDangNhap == model.TenDangNhap);

            if (tk == null || !tk.HoatDong)
                return LoiView(model, LoiDangNhap);

            var ketQua = _hasher.VerifyHashedPassword(tk, tk.MatKhauHash, model.MatKhau);
            if (ketQua == PasswordVerificationResult.Failed)
                return LoiView(model, LoiDangNhap);

            if (tk.VaiTro == null)
                return LoiView(model, "Tài khoản chưa được phân quyền");

            // Lưu thông tin người dùng vào Claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, tk.Id.ToString()),
                new(ClaimTypes.Name, tk.HoTen),
                new(ClaimTypes.Role, tk.VaiTro.TenVaiTro)
            };

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = model.GhiNho });

            return ChuyenHuong(tk.VaiTro.TenVaiTro, returnUrl);
        }

        // ĐĂNG XUẤT
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // KHÔNG CÓ QUYỀN
        [HttpGet]
        public IActionResult AccessDenied() => View();
        // ĐĂNG KÝ (GET)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return ChuyenHuong(User.FindFirstValue(ClaimTypes.Role));

            return View(new DangKyViewModel());
        }

        // ĐĂNG KÝ (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(DangKyViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return ChuyenHuong(User.FindFirstValue(ClaimTypes.Role));

            if (!ModelState.IsValid)
                return View(model);

            model.TenDangNhap = model.TenDangNhap.Trim();
            model.HoTen = model.HoTen.Trim();
            model.Email = model.Email.Trim();
            model.SoDienThoai = model.SoDienThoai?.Trim();

            if (await _db.TaiKhoans.AnyAsync(
                x => x.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError(
                    nameof(model.TenDangNhap),
                    "Tên đăng nhập đã được sử dụng.");

                return View(model);
            }

            if (await _db.TaiKhoans.AnyAsync(
                x => x.Email == model.Email))
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email đã được sử dụng.");

                return View(model);
            }

            // Tìm vai trò bạn đọc, tuyệt đối không nhận vai trò từ form
            var vaiTro = await _db.VaiTros
                .FirstOrDefaultAsync(x => x.TenVaiTro == "DocGia");

            if (vaiTro == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Chưa có vai trò DocGia trong hệ thống.");

                return View(model);
            }

            // Chọn loại độc giả mặc định
            var loaiDocGia = await _db.LoaiDocGia
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (loaiDocGia == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Chưa có loại độc giả. Vui lòng liên hệ quản trị viên.");

                return View(model);
            }

            await using var transaction =
                await _db.Database.BeginTransactionAsync();

            try
            {
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                        ? null
                        : model.SoDienThoai,
                    HoatDong = true,
                    NgayTao = DateTime.Now,
                    VaiTroId = vaiTro.Id
                };

                // Hash mật khẩu bằng cơ chế đang dùng khi đăng nhập
                taiKhoan.MatKhauHash =
                    _hasher.HashPassword(taiKhoan, model.MatKhau);

                _db.TaiKhoans.Add(taiKhoan);
                await _db.SaveChangesAsync();

                var homNay = DateOnly.FromDateTime(DateTime.Today);

                var docGia = new DocGium
                {
                    MaDocGia = "DG" + Guid.NewGuid().ToString("N")[..18],
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                        ? null
                        : model.SoDienThoai,
                    NgayDangKy = homNay,
                    NgayHetHan = homNay.AddYears(1),
                    TrangThai = "HoatDong",
                    TaiKhoanId = taiKhoan.Id,
                    LoaiDocGiaId = loaiDocGia.Id
                };

                _db.DocGia.Add(docGia);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["RegisterSuccess"] =
                    "Đăng ký thành công! Hãy đăng nhập để sử dụng thư viện.";

                return RedirectToAction(nameof(Login));
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Không thể đăng ký. Tên đăng nhập hoặc email có thể đã tồn tại.");

                return View(model);
            }
        }

        // ---------- Hàm hỗ trợ ----------

        private IActionResult LoiView(DangNhapViewModel model, string thongBao)
        {
            ModelState.AddModelError("", thongBao);
            return View(model);
        }

        // Ưu tiên returnUrl (chỉ nhận URL nội bộ), nếu không có thì theo vai trò
        private IActionResult ChuyenHuong(string? tenVaiTro, string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return tenVaiTro switch
            {
                "QuanTri" => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                "DocGia" => RedirectToAction("Index", "Home", new { area = "DocGia" }),
                "ThuThu" => RedirectToAction("Index", "Home", new { area = "ThuThu" }),
                _ => RedirectToAction("Index", "Home", new { area = "" })
            };
        }
    }
}