using Microsoft.AspNetCore.Mvc;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    public class TrangChuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ChuaPhatTrien(string chucNang = "Chức năng này")
        {
            ViewData["ChucNang"] = chucNang;
            return View();
        }
    }
}
