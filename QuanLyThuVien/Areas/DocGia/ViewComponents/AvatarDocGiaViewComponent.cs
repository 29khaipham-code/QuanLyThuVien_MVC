using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using System.Security.Claims;

namespace QuanLyThuVien.ViewComponents
{
    public class AvatarDocGiaViewComponent : ViewComponent
    {
        private readonly QuanLyThuVienContext _db;

        public AvatarDocGiaViewComponent(QuanLyThuVienContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var idClaim = HttpContext.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out var taiKhoanId))
                return View("~/Areas/DocGia/Views/Shared/Components/AvatarDocGia/Default.cshtml",
                            (string?)null);

            var avatar = await _db.DocGia
                .AsNoTracking()
                .Where(d => d.TaiKhoanId == taiKhoanId)
                .Select(d => d.AnhDaiDienUrl)
                .FirstOrDefaultAsync();

            return View("~/Areas/DocGia/Views/Shared/Components/AvatarDocGia/Default.cshtml",
                        avatar);
        }
    }
}