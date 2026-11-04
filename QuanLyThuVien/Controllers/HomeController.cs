
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using System.Diagnostics;

namespace QuanLyThuVien.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        public HomeController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DanhSachSach(
            string? tuKhoa, int? theLoaiId)
        {
            var query = _db.Saches
                .AsNoTracking()
                .Include(s => s.TheLoai)
                .Include(s => s.NhaXuatBan)
                .Include(s => s.TacGia)
                .Where(s => s.TrangThai == "HoatDong");

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(s =>
                    s.TieuDe.Contains(tuKhoa) ||
                    s.TacGia.Any(t =>
                        t.TenTacGia.Contains(tuKhoa)));
            }

            if (theLoaiId.HasValue)
            {
                query = query.Where(s =>
                    s.TheLoaiId == theLoaiId.Value);
            }

            ViewBag.TheLoais = await _db.TheLoais
                .AsNoTracking()
                .OrderBy(t => t.TenTheLoai)
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TheLoaiId = theLoaiId;

            var sach = await query
                .OrderBy(s => s.TieuDe)
                .ToListAsync();

            return View(sach);
        }

        
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> GuiYeuCau(
    [Bind("HoTen,Email,ChuDe,LoiNhan")] YeuCauHoTro model)
{
    if (!ModelState.IsValid)
    {
        TempData["SupportStatus"] = "Vui lòng nhập đầy đủ thông tin hợp lệ.";
        return RedirectToAction(nameof(Index));
    }

    _db.YeuCauHoTros.Add(model);
    await _db.SaveChangesAsync();

    TempData["SupportStatus"] = "Gửi yêu cầu hỗ trợ thành công!";
    return RedirectToAction(nameof(Index));
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DangKyNhanTin(string email)
{
    email = email?.Trim() ?? "";

    if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
        .IsValid(email))
    {
        TempData["NewsletterStatus"] = "Vui lòng nhập email hợp lệ.";
        return RedirectToAction(nameof(Index));
    }

    bool daTonTai = await _db.DangKyNhanTins
        .AnyAsync(x => x.Email == email);

    if (daTonTai)
    {
        TempData["NewsletterStatus"] = "Email này đã đăng ký rồi!";
        return RedirectToAction(nameof(Index));
    }

    _db.DangKyNhanTins.Add(new DangKyNhanTin
    {
        Email = email
    });

    await _db.SaveChangesAsync();

    TempData["NewsletterStatus"] = "Đăng ký email thành công!";
    return RedirectToAction(nameof(Index));
}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }

    }
}
