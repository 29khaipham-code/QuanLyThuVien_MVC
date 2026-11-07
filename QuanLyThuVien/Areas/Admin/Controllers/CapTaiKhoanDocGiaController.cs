
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.Admin.DocGia;
using System.Data;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class CapTaiKhoanDocGiaController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        private readonly IPasswordHasher<TaiKhoan> _hasher;

        public CapTaiKhoanDocGiaController(
            QuanLyThuVienContext db,
            IPasswordHasher<TaiKhoan> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // Hiển thị form cấp tài khoản
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var docGia = await _db.Set<DocGium>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (docGia == null)
                return NotFound();

            if (docGia.TaiKhoanId.HasValue)
            {
                TempData["Error"] =
                    "Độc giả này đã có tài khoản.";

                return RedirectToAction("Index", "DocGia");
            }

            var model = new CapTaiKhoanViewModel
            {
                DocGiaId = docGia.Id,
                MaDocGia = docGia.MaDocGia,
                HoTen = docGia.HoTen,
                Email = docGia.Email ?? ""
            };

            return View(model);
        }

        // Xử lý tạo tài khoản và liên kết
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CapTaiKhoanViewModel model)
        {
            var docGia = await _db.Set<DocGium>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == model.DocGiaId);

            if (docGia == null)
                return NotFound();

            if (docGia.TaiKhoanId.HasValue)
            {
                TempData["Error"] =
                    "Độc giả đã được liên kết tài khoản.";

                return RedirectToAction("Index", "DocGia");
            }

            model.MaDocGia = docGia.MaDocGia;
            model.HoTen = docGia.HoTen;

            // Chuẩn hóa dữ liệu
            model.TenDangNhap =
                model.TenDangNhap?.Trim() ?? "";

            model.Email = model.Email?.Trim() ?? "";

            // Chỉ kiểm tra các trường nhập liệu
            ModelState.Remove(nameof(model.MaDocGia));
            ModelState.Remove(nameof(model.HoTen));

            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra tên đăng nhập
            bool trungTen = await _db.Set<TaiKhoan>()
                .AnyAsync(x =>
                    x.TenDangNhap == model.TenDangNhap);

            if (trungTen)
            {
                ModelState.AddModelError(
                    nameof(model.TenDangNhap),
                    "Tên đăng nhập đã tồn tại.");
            }

            // Kiểm tra email
            bool trungEmail = await _db.Set<TaiKhoan>()
                .AnyAsync(x => x.Email == model.Email);

            if (trungEmail)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Email đã được sử dụng bởi tài khoản khác.");
            }

            // Vai trò Độc giả
            var vaiTro = await _db.Set<VaiTro>()
                .FirstOrDefaultAsync(
                    x => x.TenVaiTro == "DocGia");

            if (vaiTro == null)
            {
                ModelState.AddModelError(
                    "",
                    "Không tìm thấy vai trò DocGia.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Transaction: hoặc lưu cả 2, hoặc không lưu
            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                // Kiểm tra lại trong transaction
                var dg = await _db.Set<DocGium>()
                    .FirstOrDefaultAsync(
                        x => x.Id == model.DocGiaId);

                if (dg == null || dg.TaiKhoanId.HasValue)
                {
                    await transaction.RollbackAsync();

                    TempData["Error"] =
                        "Độc giả không tồn tại hoặc đã có tài khoản.";

                    return RedirectToAction(
                        "Index", "DocGia");
                }

                // Kiểm tra lại tính duy nhất
                bool daTonTai = await _db.Set<TaiKhoan>()
                    .AnyAsync(x =>
                        x.TenDangNhap == model.TenDangNhap
                        || x.Email == model.Email);

                if (daTonTai)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "Tên đăng nhập hoặc email đã được sử dụng.");

                    return View(model);
                }

                // Tạo tài khoản mới
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    HoTen = dg.HoTen,
                    Email = model.Email,
                    SoDienThoai = dg.SoDienThoai,
                    VaiTroId = vaiTro!.Id,
                    HoatDong = true,
                    NgayTao = DateTime.Now
                };

                // Băm mật khẩu trước khi lưu
                taiKhoan.MatKhauHash =
                    _hasher.HashPassword(
                        taiKhoan, model.MatKhau);

                _db.Set<TaiKhoan>().Add(taiKhoan);

                await _db.SaveChangesAsync();

                // Liên kết tài khoản với hồ sơ cũ
                dg.TaiKhoanId = taiKhoan.Id;

                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    "Cấp tài khoản thành công cho độc giả "
                    + dg.HoTen + "!";

                return RedirectToAction(
                    "Index", "DocGia");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Không thể tạo tài khoản. " +
                    "Kiểm tra dữ liệu trùng hoặc liên kết.");

                return View(model);
            }
        }
    }
}
