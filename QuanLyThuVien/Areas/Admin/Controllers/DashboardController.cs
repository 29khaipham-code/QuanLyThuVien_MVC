
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.Services;
using QuanLyThuVien.ViewModels.Admin.TongQuan;
using QuanLyThuVien.ViewModels.Admin.Dashboard;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class DashboardController : Controller
    {
        private readonly QuanLyThuVienContext _context;
        private readonly DashboardThongKeService _dashboardThongKeService;

        public DashboardController(
            QuanLyThuVienContext context,
            DashboardThongKeService dashboardThongKeService)
        {
            _context = context;
            _dashboardThongKeService = dashboardThongKeService;
        }

        // Trang tổng quan của Admin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Tổng quan";

            DashboardThongKeViewModel thongKe =
                await _dashboardThongKeService.LayThongKeAsync();

            return View(thongKe);
        }

        // Trang tạm cho chức năng chưa triển khai
        [HttpGet]
        public async Task<IActionResult> Module(string id)
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

            if (id == "ThongKe")
            {
                var model = await TaoThongKeAsync();
                return View("ThongKe", model);
            }

            return View();
        }

        private async Task<ThongKeViewModel> TaoThongKeAsync()
        {
            var homNay = DateOnly.FromDateTime(DateTime.Today);
            var thangHienTai = new DateOnly(homNay.Year, homNay.Month, 1);
            var thangBatDau = thangHienTai.AddMonths(-5);

            var model = new ThongKeViewModel
            {
                TongDauSach = await _context.Saches.CountAsync(),
                TongDocGia = await _context.DocGia.CountAsync(),
                SachDangMuon = await _context.ChiTietMuons
                    .CountAsync(x => x.NgayTra == null),
                SachQuaHan = await _context.ChiTietMuons
                    .CountAsync(x => x.NgayTra == null && x.HanTra < homNay),
            };

            // Lấy ngày mượn trong 6 tháng gần nhất, sau đó gom nhóm trong bộ nhớ.
            // Cách này tránh phụ thuộc vào khả năng dịch DateOnly.Year/Month của SQL Server.
            var ngayMuonTrongKy = await _context.PhieuMuons
                .AsNoTracking()
                .Where(x => x.NgayMuon >= thangBatDau)
                .Select(x => x.NgayMuon)
                .ToListAsync();

            for (var i = 0; i < 6; i++)
            {
                var thang = thangBatDau.AddMonths(i);
                var soLuotMuon = ngayMuonTrongKy.Count(x =>
                    x.Year == thang.Year && x.Month == thang.Month);

                model.MuonTheoThang.Add(new ThongKeTheoThangViewModel
                {
                    Thang = $"T{thang.Month:00}/{thang.Year}",
                    SoLuotMuon = soLuotMuon
                });
            }

            model.SachTheoTheLoai = await _context.Saches
                .AsNoTracking()
                .GroupBy(x => x.TheLoai.TenTheLoai)
                .Select(group => new ThongKeTheLoaiViewModel
                {
                    TenTheLoai = group.Key,
                    SoDauSach = group.Count()
                })
                .OrderByDescending(x => x.SoDauSach)
                .Take(6)
                .ToListAsync();

            model.SachMuonNhieu = await _context.ChiTietMuons
                .AsNoTracking()
                .GroupBy(x => x.BanSao.Sach.TieuDe)
                .Select(group => new SachMuonNhieuViewModel
                {
                    TenSach = group.Key,
                    SoLuotMuon = group.Count()
                })
                .OrderByDescending(x => x.SoLuotMuon)
                .Take(5)
                .ToListAsync();

            return model;
        }
    }
}
