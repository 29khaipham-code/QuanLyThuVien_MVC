using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    // Lớp cha cho các controller của Area Độc giả.
    // Không đặt [Authorize] ở đây: mỗi controller tự khai báo
    //   - [AllowAnonymous]                  cho trang khách được xem (Home, Sach)
    //   - [Authorize(Roles = "DocGia")]     cho trang bắt buộc đăng nhập (MuonTra, DatTruoc, HoSo)
    [Area("DocGia")]
    public abstract class DocGiaControllerBase : Controller
    {
        protected readonly QuanLyThuVienContext Db;

        private DocGium? _docGia;
        private bool _daNap;

        protected DocGiaControllerBase(QuanLyThuVienContext db)
        {
            Db = db;
        }

        // True nếu người đang truy cập đã đăng nhập bằng vai trò DocGia
        protected bool LaDocGia => User.IsInRole("DocGia");

        // Hồ sơ độc giả của người đang đăng nhập.
        // Trả về null nếu là khách, nhân viên (không phải DocGia) hoặc tài khoản chưa có hồ sơ.
        protected async Task<DocGium?> LayDocGiaAsync()
        {
            if (_daNap) return _docGia;
            _daNap = true;

            if (!LaDocGia) return null;

            if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId))
            {
                _docGia = await Db.DocGia.AsNoTracking()
                    .Include(d => d.LoaiDocGia)
                    .FirstOrDefaultAsync(d => d.TaiKhoanId == taiKhoanId);
            }

            return _docGia;
        }

        // Trang báo "chưa có hồ sơ độc giả"
        protected IActionResult ChuaCoHoSo() => View("ChuaCoHoSo");
    }
}
