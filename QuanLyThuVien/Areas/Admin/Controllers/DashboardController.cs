
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class DashboardController : Controller
    {
        // Trang tổng quan của Admin
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Tổng quan";
            return View();
        }

        // Trang tạm cho chức năng chưa triển khai
        [HttpGet]
        public IActionResult Module(string id)
        {
            var modules = new Dictionary<string, string>
            {
                ["TaiKhoan"] = "Quản lý tài khoản",
                ["TheLoai"] = "Quản lý thể loại",
                ["TacGia"] = "Quản lý tác giả",
                ["NhaXuatBan"] = "Quản lý nhà xuất bản",
                ["Sach"] = "Quản lý sách",
                ["BanSaoSach"] = "Quản lý bản sao sách",
                ["DocGia"] = "Quản lý độc giả",
                ["MuonTra"] = "Quản lý mượn trả",
                ["DatTruoc"] = "Quản lý đặt trước",
                ["Phat"] = "Quản lý tiền phạt",
                ["SachDienTu"] = "Sách điện tử",
                ["ThongKe"] = "Thống kê báo cáo"
            };

            if (string.IsNullOrWhiteSpace(id) ||
                !modules.TryGetValue(id, out var title))
            {
                return NotFound();
            }

            ViewData["Title"] = title;
            return View();
        }
    }
}
