using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.HoSo;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    public class HoSoController : DocGiaBaseController
    {
        private readonly IPasswordHasher<TaiKhoan> _hasher;

        public HoSoController(QuanLyThuVienContext db, IPasswordHasher<TaiKhoan> hasher)
            : base(db)
        {
            _hasher = hasher;
        }

        // ================= XEM THÔNG TIN =================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dg = await LayDocGiaHienTai();
            if (dg == null) return Forbid();

            var tk = await _db.TaiKhoans.FindAsync(dg.TaiKhoanId);
            return View(TaoViewModel(dg, tk!));
        }

        // ================= CẬP NHẬT THÔNG TIN =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(HoSoViewModel model)
        {
            var dg = await LayDocGiaHienTai();
            if (dg == null) return Forbid();
            var tk = (await _db.TaiKhoans.FindAsync(dg.TaiKhoanId))!;

            // Kiểm tra nghiệp vụ
            if (model.NgaySinh.HasValue && model.NgaySinh.Value > DateOnly.FromDateTime(DateTime.Today))
                ModelState.AddModelError(nameof(model.NgaySinh), "Ngày sinh không được ở tương lai");

            if (await _db.TaiKhoans.AnyAsync(x => x.Email == model.Email && x.Id != tk.Id))
                ModelState.AddModelError(nameof(model.Email), "Email đã được tài khoản khác sử dụng");

            if (!ModelState.IsValid)
            {
                // Điền lại phần chỉ hiển thị vì form không gửi lên
                var hienThi = TaoViewModel(dg, tk);
                model.MaDocGia = hienThi.MaDocGia;
                model.TenDangNhap = hienThi.TenDangNhap;
                model.LoaiDocGia = hienThi.LoaiDocGia;
                model.SoSachToiDa = hienThi.SoSachToiDa;
                model.SoNgayMuonToiDa = hienThi.SoNgayMuonToiDa;
                model.NgayDangKy = hienThi.NgayDangKy;
                model.NgayHetHan = hienThi.NgayHetHan;
                model.TrangThai = hienThi.TrangThai;
                return View(model);
            }

            // Chỉ gán các trường cho phép sửa (tránh over-posting)
            dg.HoTen = model.HoTen.Trim();
            dg.Email = model.Email.Trim();
            dg.SoDienThoai = model.SoDienThoai;
            dg.NgaySinh = model.NgaySinh;
            dg.GioiTinh = model.GioiTinh;
            dg.DiaChi = model.DiaChi;

            tk.HoTen = dg.HoTen;
            tk.Email = dg.Email;
            tk.SoDienThoai = dg.SoDienThoai;

            await _db.SaveChangesAsync();
            await LamMoiCookieTen(tk.HoTen);

            TempData["ThongBao"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(Index));
        }

        // ================= ĐỔI MẬT KHẨU =================
        [HttpGet]
        public IActionResult DoiMatKhau() => View(new DoiMatKhauViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var tkId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var tk = await _db.TaiKhoans.FindAsync(tkId);
            if (tk == null) return Forbid();

            var kq = _hasher.VerifyHashedPassword(tk, tk.MatKhauHash, model.MatKhauCu);
            if (kq == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.MatKhauCu), "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            if (model.MatKhauMoi == model.MatKhauCu)
            {
                ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu mới phải khác mật khẩu cũ");
                return View(model);
            }

            tk.MatKhauHash = _hasher.HashPassword(tk, model.MatKhauMoi);
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ================= HÀM PHỤ =================
        private static HoSoViewModel TaoViewModel(DocGium dg, TaiKhoan tk) => new()
        {
            HoTen = dg.HoTen,
            Email = dg.Email ?? tk.Email,
            SoDienThoai = dg.SoDienThoai,
            NgaySinh = dg.NgaySinh,
            GioiTinh = dg.GioiTinh,
            DiaChi = dg.DiaChi,
            MaDocGia = dg.MaDocGia,
            TenDangNhap = tk.TenDangNhap,
            LoaiDocGia = dg.LoaiDocGia.TenLoai,
            SoSachToiDa = dg.LoaiDocGia.SoSachToiDa,
            SoNgayMuonToiDa = dg.LoaiDocGia.SoNgayMuonToiDa,
            NgayDangKy = dg.NgayDangKy,
            NgayHetHan = dg.NgayHetHan,
            TrangThai = dg.TrangThai
        };

        // Tên hiển thị nằm trong cookie, đổi họ tên xong phải cấp lại cookie
        private async Task LamMoiCookieTen(string hoTen)
        {
            var claims = User.Claims.Where(c => c.Type != ClaimTypes.Name).ToList();
            claims.Add(new Claim(ClaimTypes.Name, hoTen));

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            var cu = await HttpContext.AuthenticateAsync();   // giữ nguyên "ghi nhớ đăng nhập"
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, principal, cu.Properties);
        }
    }
}