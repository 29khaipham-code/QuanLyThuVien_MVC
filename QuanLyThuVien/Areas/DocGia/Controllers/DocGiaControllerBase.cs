using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    // Lớp cha cho các controller của Area Độc giả: lấy hồ sơ độc giả của người đang đăng nhập
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public abstract class DocGiaControllerBase : Controller
    {
        protected readonly QuanLyThuVienContext Db;

        private DocGium? _docGia;
        private bool _daNap;

        protected DocGiaControllerBase(QuanLyThuVienContext db)
        {
            Db = db;
        }

        // Hồ sơ độc giả gắn với tài khoản đang đăng nhập (null nếu chưa có hồ sơ). Chỉ truy vấn một lần mỗi request.
        protected async Task<DocGium?> LayDocGiaAsync()
        {
            if (_daNap) return _docGia;
            _daNap = true;

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
