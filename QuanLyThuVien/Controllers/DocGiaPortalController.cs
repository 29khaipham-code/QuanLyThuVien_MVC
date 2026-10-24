using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyThuVien.Controllers
{
    [Authorize(Roles = "DocGia")]
    public class DocGiaPortalController : Controller
    {
        // Các chức năng dành riêng cho độc giả
    }
}
