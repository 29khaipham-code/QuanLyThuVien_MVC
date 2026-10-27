using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public abstract class DocGiaBaseController : Controller
    {
        protected readonly QuanLyThuVienContext _db;
        protected DocGiaBaseController(QuanLyThuVienContext db) => _db = db;

        // Không bao giờ nhận docGiaId từ URL/form, luôn lấy từ tài khoản đăng nhập
        protected async Task<DocGium?> LayDocGiaHienTai()
        {
            var tkId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return await _db.DocGia
                .Include(d => d.LoaiDocGia)
                .FirstOrDefaultAsync(d => d.TaiKhoanId == tkId);
        }
    }
}