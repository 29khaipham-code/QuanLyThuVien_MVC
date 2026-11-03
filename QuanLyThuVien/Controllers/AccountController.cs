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
                // ThuThu: tạm về trang khách cho đến khi có Area ThuThu
                _ => RedirectToAction("Index", "Home", new { area = "" })
            };
        }
    }
}