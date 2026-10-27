
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels;

namespace QuanLyThuVien.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        private readonly IPasswordHasher<TaiKhoan> _hasher;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            QuanLyThuVienContext db,
            IPasswordHasher<TaiKhoan> hasher,
            IWebHostEnvironment env)
        {
            _db = db;
            _hasher = hasher;
            _env = env;
        }

        // =========================================
        // 1. HIỂN THỊ TRANG ĐĂNG NHẬP
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.IsInRole("QuanTri"))
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Admin" });
            }

            return View(new DangNhapViewModel());
        }

        // =========================================
        // 2. XỬ LÝ ĐĂNG NHẬP
        // =========================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            DangNhapViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (_env.IsDevelopment())
                {
                    Debug.WriteLine(
                        "LOGIN DEBUG - ModelState khong hop le");
                }

                return View(model);
            }

            // Tìm tài khoản trong SQL Server
            var tk = await _db.TaiKhoans
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x =>
                    x.TenDangNhap == model.TenDangNhap);

            // =====================================
            // 3. KIỂM TRA DEBUG
            // Chỉ chạy ở môi trường Development
            // =====================================
            if (_env.IsDevelopment())
            {
                Debug.WriteLine("");
                Debug.WriteLine("========== LOGIN DEBUG ==========");

                Debug.WriteLine(
                    "Tim thay tai khoan: " + (tk != null));

                Debug.WriteLine(
                    "Tai khoan hoat dong: " +
                    tk?.HoatDong);

                Debug.WriteLine(
                    "Do dai ten dang nhap: " +
                    model.TenDangNhap?.Length);

                Debug.WriteLine(
                    "Do dai mat khau nhap: " +
                    model.MatKhau?.Length);

                // Tạo dấu vân tay của hash để so sánh
                // giữa hai laptop. Không in hash gốc.
                if (tk != null &&
                    !string.IsNullOrEmpty(tk.MatKhauHash))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(
                        tk.MatKhauHash);

                    byte[] fingerprintBytes =
                        SHA256.HashData(bytes);

                    string fingerprint =
                        Convert.ToHexString(fingerprintBytes);

                    Debug.WriteLine(
                        "Hash fingerprint: " + fingerprint);
                }
            }

            // =====================================
            // 4. KIỂM TRA TÀI KHOẢN
            // =====================================
            if (tk == null || !tk.HoatDong)
            {
                ModelState.AddModelError(
                    "",
                    "Tài khoản hoặc mật khẩu không đúng");

                return View(model);
            }

            // =====================================
            // 5. KIỂM TRA MẬT KHẨU
            // =====================================
            var ketQua = _hasher.VerifyHashedPassword(
                tk,
                tk.MatKhauHash,
                model.MatKhau);

            if (_env.IsDevelopment())
            {
                Debug.WriteLine(
                    "Ket qua xac minh mat khau: " +
                    ketQua);

                Debug.WriteLine(
                    "Vai tro: " +
                    tk.VaiTro?.TenVaiTro);

                Debug.WriteLine(
                    "=================================");
            }

            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Tài khoản hoặc mật khẩu không đúng");

                return View(model);
            }

            // =====================================
            // 6. KIỂM TRA VAI TRÒ
            // =====================================
            if (tk.VaiTro == null)
            {
                ModelState.AddModelError(
                    "",
                    "Tài khoản chưa được phân quyền");

                return View(model);
            }

            // =====================================
            // 7. TẠO CLAIMS
            // =====================================
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    tk.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    tk.HoTen),

                new Claim(
                    ClaimTypes.Role,
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

            // =====================================
            // 8. LƯU COOKIE ĐĂNG NHẬP
            // =====================================
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            // =====================================
            // 9. CHUYỂN HƯỚNG THEO VAI TRÒ
            // =====================================
            if (tk.VaiTro.TenVaiTro == "QuanTri")
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard",
                    new { area = "Admin" });
            }

            // ThuThu và DocGia tạm thời về Home
            return RedirectToAction(
                "Index",
                "Home",
                new { area = "" });
        }

        // =========================================
        // 10. ĐĂNG XUẤT
        // =========================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        // =========================================
        // 11. TRANG KHÔNG CÓ QUYỀN
        // =========================================
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
