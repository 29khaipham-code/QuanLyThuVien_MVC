
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Services;
using QuanLyThuVien.ViewModels.Admin.TongQuan;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class DashboardController : Controller
    {
        private readonly DashboardThongKeService _thongKeService;

        public DashboardController(
            DashboardThongKeService thongKeService)
        {
            _thongKeService = thongKeService;
        }

        // TRANG TONG QUAN ADMIN
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Tổng quan";

            DashboardThongKeViewModel model =
                await _thongKeService.LayThongKeAsync();

            return View(model);
        }

        // TRANG TAM CHO CHUC NANG CHUA TRIEN KHAI
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
