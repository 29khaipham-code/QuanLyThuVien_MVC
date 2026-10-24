
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels;

namespace QuanLyThuVien.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        private readonly IPasswordHasher<TaiKhoan> _hasher;

        public AccountController(
            QuanLyThuVienContext db,
            IPasswordHasher<TaiKhoan> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // HIỂN THỊ TRANG ĐĂNG NHẬP
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new DangNhapViewModel());
        }

        // XỬ LÝ ĐĂNG NHẬP
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            DangNhapViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tk = await _db.TaiKhoans
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x =>
                    x.TenDangNhap == model.TenDangNhap);

            if (tk == null || !tk.HoatDong)
            {
                ModelState.AddModelError(
                    "", "Tài khoản hoặc mật khẩu không đúng");
                return View(model);
            }

            var ketQua = _hasher.VerifyHashedPassword(
                tk, tk.MatKhauHash, model.MatKhau);

            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "", "Tài khoản hoặc mật khẩu không đúng");
                return View(model);
            }

            // Lưu thông tin người dùng vào Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,
                    tk.Id.ToString()),

                new Claim(ClaimTypes.Name, tk.HoTen),

                new Claim(ClaimTypes.Role,
                    tk.VaiTro.TenVaiTro)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.GhiNho
            };

            // Tạo Cookie đăng nhập
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // ĐĂNG XUẤT
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        // KHÔNG CÓ QUYỀN
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
