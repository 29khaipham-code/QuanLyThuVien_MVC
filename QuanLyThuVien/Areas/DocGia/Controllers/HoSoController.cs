using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.DocGia;
using System.Security.Claims;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public class HoSoController : DocGiaControllerBase
    {
        private const long ToiDaDungLuongAnh = 2 * 1024 * 1024;   // 2 MB
        private const string ThuMucAnh = "uploads/avatars";       // tương đối so với wwwroot
        private static readonly string[] DuoiAnhChoPhep = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly IPasswordHasher<TaiKhoan> _hasher;
        private readonly IWebHostEnvironment _env;

        public HoSoController(
            QuanLyThuVienContext db,
            IPasswordHasher<TaiKhoan> hasher,
            IWebHostEnvironment env) : base(db)
        {
            _hasher = hasher;
            _env = env;
        }

        // XEM HỒ SƠ
        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dg = await TaiDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            var vm = new HoSoViewModel
            {
                TenDangNhap = dg.TaiKhoan?.TenDangNhap ?? "",
                MaDocGia = dg.MaDocGia,
                HoTen = dg.HoTen,
                NgaySinh = dg.NgaySinh,
                GioiTinh = dg.GioiTinh,
                Email = dg.Email,
                SoDienThoai = dg.SoDienThoai,
                DiaChi = dg.DiaChi,
                AnhDaiDienUrl = dg.AnhDaiDienUrl,
                LoaiDocGia = dg.LoaiDocGia.TenLoai,
                SoSachToiDa = dg.LoaiDocGia.SoSachToiDa,
                SoNgayMuonToiDa = dg.LoaiDocGia.SoNgayMuonToiDa,
                NgayDangKy = dg.NgayDangKy,
                NgayHetHan = dg.NgayHetHan,
                TrangThai = dg.TrangThai
            };

            var luotMuon = Db.ChiTietMuons.AsNoTracking()
                .Where(c => c.PhieuMuon.DocGiaId == dg.Id && c.PhieuMuon.TrangThai != "Huy");

            vm.TongLuotMuon = await luotMuon.CountAsync();
            vm.DangMuon = await luotMuon.CountAsync(c => c.NgayTra == null);
            vm.PhatChuaNop = await Db.Phats.AsNoTracking()
                .Where(p => p.ChiTietMuon.PhieuMuon.DocGiaId == dg.Id && !p.DaThanhToan)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;

            return View(vm);
        }

        
        // SỬA THÔNG TIN CÁ NHÂN
        
        [HttpGet]
        public async Task<IActionResult> Sua()
        {
            var dg = await TaiDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            return View(new SuaHoSoViewModel
            {
                MaDocGia = dg.MaDocGia,
                TenDangNhap = dg.TaiKhoan?.TenDangNhap ?? "",
                HoTen = dg.HoTen,
                NgaySinh = dg.NgaySinh,
                GioiTinh = dg.GioiTinh,
                Email = dg.Email ?? dg.TaiKhoan?.Email ?? "",
                SoDienThoai = dg.SoDienThoai,
                DiaChi = dg.DiaChi
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(SuaHoSoViewModel model)
        {
            var dg = await TaiDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            // Các trường chỉ đọc luôn lấy lại từ dữ liệu thật (không tin dữ liệu gửi lên)
            model.MaDocGia = dg.MaDocGia;
            model.TenDangNhap = dg.TaiKhoan?.TenDangNhap ?? "";

            var email = model.Email?.Trim() ?? "";

            if (model.NgaySinh.HasValue &&
                (model.NgaySinh > DateOnly.FromDateTime(DateTime.Today) || model.NgaySinh < new DateOnly(1900, 1, 1)))
            {
                ModelState.AddModelError(nameof(model.NgaySinh), "Ngày sinh không hợp lệ");
            }

            // Email của tài khoản là duy nhất
            if (email.Length > 0 &&
                await Db.TaiKhoans.AnyAsync(t => t.Email == email && t.Id != dg.TaiKhoanId))
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được dùng bởi tài khoản khác");
            }

            if (!ModelState.IsValid)
                return View(model);

            var hoTen = model.HoTen.Trim();
            var soDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim();

            dg.HoTen = hoTen;
            dg.NgaySinh = model.NgaySinh;
            dg.GioiTinh = string.IsNullOrWhiteSpace(model.GioiTinh) ? null : model.GioiTinh;
            dg.Email = email;
            dg.SoDienThoai = soDienThoai;
            dg.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim();

            // Đồng bộ sang tài khoản đăng nhập
            if (dg.TaiKhoan != null)
            {
                dg.TaiKhoan.HoTen = hoTen;
                dg.TaiKhoan.Email = email;
                dg.TaiKhoan.SoDienThoai = soDienThoai;
            }

            try
            {
                await Db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Không lưu được thông tin (có thể email đã bị trùng). Vui lòng thử lại.");
                return View(model);
            }

            // Tên hiển thị trên navbar lấy từ cookie nên cần làm mới cookie
            await LamMoiCookieAsync(hoTen);

            TempData["ThanhCong"] = "Đã cập nhật thông tin cá nhân.";
            return RedirectToAction(nameof(Index));
        }

        
        // ĐỔI MẬT KHẨU
        
        [HttpGet]
        public IActionResult DoiMatKhau() => View(new DoiMatKhauViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId))
                return Forbid();

            var tk = await Db.TaiKhoans.FirstOrDefaultAsync(t => t.Id == taiKhoanId);
            if (tk == null) return Forbid();

            var ketQua = _hasher.VerifyHashedPassword(tk, tk.MatKhauHash, model.MatKhauHienTai);
            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.MatKhauHienTai), "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            if (model.MatKhauMoi == model.MatKhauHienTai)
            {
                ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu mới phải khác mật khẩu hiện tại");
                return View(model);
            }

            tk.MatKhauHash = _hasher.HashPassword(tk, model.MatKhauMoi);
            await Db.SaveChangesAsync();

            TempData["ThanhCong"] = "Đã đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        
        // ẢNH ĐẠI DIỆN
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiAnhDaiDien(IFormFile? anh)
        {
            var dg = await TaiDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            IActionResult Loi(string thongBao)
            {
                TempData["Loi"] = thongBao;
                return RedirectToAction(nameof(Index));
            }

            if (anh == null || anh.Length == 0)
                return Loi("Vui lòng chọn một ảnh.");

            if (anh.Length > ToiDaDungLuongAnh)
                return Loi("Ảnh quá lớn, tối đa 2 MB.");

            var duoi = Path.GetExtension(anh.FileName).ToLowerInvariant();
            if (!DuoiAnhChoPhep.Contains(duoi))
                return Loi("Chỉ chấp nhận ảnh JPG, PNG hoặc WEBP.");

            await using var luong = anh.OpenReadStream();

            // Kiểm tra nội dung thật của tệp (không chỉ tin vào đuôi tệp)
            if (!LaAnhHopLe(luong, duoi))
                return Loi("Tệp không phải là ảnh hợp lệ.");
            luong.Position = 0;

            // Lưu với tên ngẫu nhiên để không trùng và không dùng tên do người dùng gửi lên
            var thuMuc = Path.Combine(_env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(thuMuc);

            var tenFile = $"{Guid.NewGuid():N}{duoi}";
            await using (var fs = System.IO.File.Create(Path.Combine(thuMuc, tenFile)))
            {
                await luong.CopyToAsync(fs);
            }

            var anhCu = dg.AnhDaiDienUrl;
            dg.AnhDaiDienUrl = $"{ThuMucAnh}/{tenFile}";
            await Db.SaveChangesAsync();

            XoaFileAnh(anhCu);

            TempData["ThanhCong"] = "Đã cập nhật ảnh đại diện.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaAnhDaiDien()
        {
            var dg = await TaiDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            var anhCu = dg.AnhDaiDienUrl;
            dg.AnhDaiDienUrl = null;
            await Db.SaveChangesAsync();

            XoaFileAnh(anhCu);

            TempData["ThanhCong"] = "Đã xóa ảnh đại diện.";
            return RedirectToAction(nameof(Index));
        }

        
        // HÀM HỖ TRỢ
        

        // Hồ sơ độc giả có theo dõi thay đổi (để sửa), kèm tài khoản và loại độc giả
        private async Task<DocGium?> TaiDocGiaAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId))
                return null;

            return await Db.DocGia
                .Include(d => d.TaiKhoan)
                .Include(d => d.LoaiDocGia)
                .FirstOrDefaultAsync(d => d.TaiKhoanId == taiKhoanId);
        }

        // Đăng nhập lại với tên mới nhưng giữ nguyên các claim khác và kiểu cookie (ghi nhớ hay không)
        private async Task LamMoiCookieAsync(string hoTen)
        {
            var claims = User.Claims
                .Where(c => c.Type != ClaimTypes.Name)
                .Append(new Claim(ClaimTypes.Name, hoTen))
                .ToList();

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            var ketQuaCu = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var props = ketQuaCu?.Properties ?? new AuthenticationProperties();

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        }

        // Kiểm tra "chữ ký" đầu tệp: JPEG, PNG, WEBP
        private static bool LaAnhHopLe(Stream luong, string duoi)
        {
            var dau = new byte[12];
            var doc = luong.Read(dau, 0, dau.Length);
            if (doc < dau.Length) return false;

            return duoi switch
            {
                ".jpg" or ".jpeg" => dau[0] == 0xFF && dau[1] == 0xD8 && dau[2] == 0xFF,
                ".png" => dau[0] == 0x89 && dau[1] == 0x50 && dau[2] == 0x4E && dau[3] == 0x47
                       && dau[4] == 0x0D && dau[5] == 0x0A && dau[6] == 0x1A && dau[7] == 0x0A,
                ".webp" => dau[0] == 'R' && dau[1] == 'I' && dau[2] == 'F' && dau[3] == 'F'
                        && dau[8] == 'W' && dau[9] == 'E' && dau[10] == 'B' && dau[11] == 'P',
                _ => false
            };
        }

        // Xóa tệp ảnh cũ, chỉ trong thư mục avatars
        private void XoaFileAnh(string? url)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith(ThuMucAnh + "/"))
                return;

            var duongDan = Path.Combine(_env.WebRootPath, "uploads", "avatars", Path.GetFileName(url));

            try
            {
                if (System.IO.File.Exists(duongDan))
                    System.IO.File.Delete(duongDan);
            }
            catch (IOException)
            {
                // Tệp đang bị khóa: bỏ qua, không làm hỏng thao tác chính
            }
        }
    }
}
