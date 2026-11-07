using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Areas.ThuThu.Controllers
{
    [Area("ThuThu")]
    [Authorize(Roles = "ThuThu")]
    public class HomeController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        public HomeController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Trang tổng quan Thủ thư";

            var homNay = DateOnly.FromDateTime(DateTime.Today);

            ViewBag.TongDauSach = await _db.Saches.CountAsync();
            ViewBag.TongDocGia = await _db.DocGia.CountAsync();
            ViewBag.SachDangMuon = await _db.ChiTietMuons
                .CountAsync(x => x.NgayTra == null);
            ViewBag.SachQuaHan = await _db.ChiTietMuons
                .CountAsync(x => x.NgayTra == null && x.HanTra < homNay);

            return View();
        }
    }
}
