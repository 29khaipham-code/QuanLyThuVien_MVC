using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    public class TongQuanController : DocGiaBaseController
    {
        public TongQuanController(QuanLyThuVienContext db) : base(db) { }

        public async Task<IActionResult> Index()
        {
            var dg = await LayDocGiaHienTai();
            if (dg == null) return Forbid();

            var homNay = DateOnly.FromDateTime(DateTime.Today);

            var dangMuon = _db.ChiTietMuons
                .Where(c => c.PhieuMuon.DocGiaId == dg.Id && c.NgayTra == null);

            ViewBag.DocGia = dg;
            ViewBag.DangMuon = await dangMuon.CountAsync();
            ViewBag.QuaHan = await dangMuon.CountAsync(c => c.HanTra < homNay);
            ViewBag.PhatChuaNop = await _db.Phats
                .Where(p => !p.DaThanhToan && p.ChiTietMuon.PhieuMuon.DocGiaId == dg.Id)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;
            ViewBag.DatTruoc = await _db.DatTruocs
                .CountAsync(d => d.DocGiaId == dg.Id
                    && (d.TrangThai == "DangCho" || d.TrangThai == "DaGiu"));
            return View();
        }
    }
}