using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyThuVien.Controllers
{
    [Authorize(Roles = "QuanTri")]
    public class TaiKhoanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
