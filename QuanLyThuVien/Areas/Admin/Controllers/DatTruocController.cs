
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.DatTruoc;

// Đặt bí danh để phân biệt Entity và namespace ViewModel
using DatTruocEntity = QuanLyThuVien.Models.DatTruoc;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,ThuThu")]
    public class DatTruocController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        public DatTruocController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        // ========================================
        // 1. DANH SÁCH + TÌM KIẾM + PHÂN TRANG
        // ========================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword,
            string? trangThai,
            int page = 1)
        {
            const int pageSize = 10;
            page = Math.Max(1, page);
            keyword = keyword?.Trim();

            var query =
                from dt in _db.Set<DatTruocEntity>().AsNoTracking()
                join dg in _db.Set<DocGium>().AsNoTracking()
                    on dt.DocGiaId equals dg.Id
                join s in _db.Set<Sach>().AsNoTracking()
                    on dt.SachId equals s.Id
                join bs0 in _db.Set<BanSaoSach>().AsNoTracking()
                    on dt.BanSaoGiuId equals (int?)bs0.Id
                    into banSaoGroup
                from bs in banSaoGroup.DefaultIfEmpty()
                select new DatTruocListItemViewModel
                {
                    Id = dt.Id,
                    DocGiaId = dt.DocGiaId,
                    SachId = dt.SachId,
                    MaDocGia = dg.MaDocGia,
                    HoTen = dg.HoTen,
                    TenSach = s.TieuDe,
                    NgayDat = dt.NgayDat,
                    NgayHetHan = dt.NgayHetHan,
                    BanSaoGiuId = dt.BanSaoGiuId,
                    MaVach = bs == null ? null : bs.MaVach,
                    TrangThai = dt.TrangThai
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaDocGia.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    x.TenSach.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x =>
                    x.TrangThai == trangThai);
            }

            int total = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                total / (double)pageSize);

            if (totalPages > 0)
                page = Math.Min(page, totalPages);

            var data = await query
                .OrderByDescending(x => x.NgayDat)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangThai;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            return View(data);
        }

        // ========================================
        // 2. LOAD COMBOBOX
        // ========================================
        private async Task LoadOptions()
        {
            var docGias = await _db.Set<DocGium>()
                .AsNoTracking()
                .Where(x => x.TrangThai == "HoatDong"
                    && x.NgayHetHan >=
                        DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(x => x.HoTen)
                .Select(x => new
                {
                    x.Id,
                    HienThi = x.MaDocGia + " - " + x.HoTen
                })
                .ToListAsync();

            ViewBag.DocGiaOptions = new SelectList(
                docGias, "Id", "HienThi");

            var saches = await _db.Set<Sach>()
                .AsNoTracking()
                .Where(x => x.TrangThai == "HoatDong")
                .OrderBy(x => x.TieuDe)
                .Select(x => new
                {
                    x.Id,
                    x.TieuDe
                })
                .ToListAsync();

            ViewBag.SachOptions = new SelectList(
                saches, "Id", "TieuDe");
        }

        // ========================================
        // 3. FORM THÊM YÊU CẦU
        // ========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOptions();
            return View(new DatTruocFormViewModel());
        }

        // ========================================
        // 4. TẠO YÊU CẦU ĐẶT TRƯỚC
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DatTruocFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            var dg = await _db.Set<DocGium>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == model.DocGiaId);

            if (dg == null ||
                dg.TrangThai != "HoatDong" ||
                dg.NgayHetHan < today)
            {
                ModelState.AddModelError(
                    nameof(model.DocGiaId),
                    "Độc giả không tồn tại, bị khóa hoặc hết hạn.");
            }

            bool sachHopLe = await _db.Set<Sach>()
                .AnyAsync(x =>
                    x.Id == model.SachId &&
                    x.TrangThai == "HoatDong");

            if (!sachHopLe)
            {
                ModelState.AddModelError(
                    nameof(model.SachId),
                    "Đầu sách không tồn tại hoặc ngừng phục vụ.");
            }

            bool daDat = await _db.Set<DatTruocEntity>()
                .AnyAsync(x =>
                    x.DocGiaId == model.DocGiaId &&
                    x.SachId == model.SachId &&
                    (x.TrangThai == "DangCho" ||
                     x.TrangThai == "DaGiu"));

            if (daDat)
            {
                ModelState.AddModelError(
                    "",
                    "Độc giả đã có yêu cầu đặt trước " +
                    "đang chờ hoặc đang giữ cuốn sách này.");
            }

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            var datTruoc = new DatTruocEntity
            {
                DocGiaId = model.DocGiaId,
                SachId = model.SachId,
                NgayDat = DateTime.Now,
                NgayHetHan = null,
                BanSaoGiuId = null,
                TrangThai = "DangCho"
            };

            _db.Set<DatTruocEntity>().Add(datTruoc);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể tạo yêu cầu. Có thể " +
                    "độc giả đã đặt trùng sách.");

                await LoadOptions();
                return View(model);
            }

            TempData["Success"] =
                "Tạo yêu cầu đặt trước thành công!";

            return RedirectToAction(nameof(Index));
        }

        // ========================================
        // 5. GIỮ BẢN SAO CHO ĐỘC GIẢ
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hold(int id)
        {
            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var dat = await _db.Set<DatTruocEntity>()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (dat == null)
                    return NotFound();

                if (dat.TrangThai != "DangCho")
                {
                    TempData["Error"] =
                        "Chỉ có thể giữ sách cho yêu cầu đang chờ.";
                    return RedirectToAction(nameof(Index));
                }

                var today = DateOnly.FromDateTime(DateTime.Today);

                bool theHopLe = await _db.Set<DocGium>()
                    .AnyAsync(x =>
                        x.Id == dat.DocGiaId &&
                        x.TrangThai == "HoatDong" &&
                        x.NgayHetHan >= today);

                bool sachHopLe = await _db.Set<Sach>()
                    .AnyAsync(x =>
                        x.Id == dat.SachId &&
                        x.TrangThai == "HoatDong");

                if (!theHopLe || !sachHopLe)
                {
                    TempData["Error"] =
                        "Thẻ độc giả hoặc đầu sách không còn hợp lệ.";
                    return RedirectToAction(nameof(Index));
                }

                // Tôn trọng thứ tự đặt trước theo thời gian.
                var dauHang = await _db.Set<DatTruocEntity>()
                    .Where(x =>
                        x.SachId == dat.SachId &&
                        x.TrangThai == "DangCho")
                    .OrderBy(x => x.NgayDat)
                    .ThenBy(x => x.Id)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (dauHang != dat.Id)
                {
                    TempData["Error"] =
                        "Còn độc giả đặt trước sớm hơn. " +
                        "Hãy xử lý theo thứ tự hàng chờ.";
                    return RedirectToAction(nameof(Index));
                }

                // Chọn bản sao còn sẵn sàng.
                var banSao = await _db.Set<BanSaoSach>()
                    .Where(x =>
                        x.SachId == dat.SachId &&
                        x.TrangThai == "SanSang")
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (banSao == null)
                {
                    TempData["Error"] =
                        "Hiện không có bản sao sẵn sàng.";
                    return RedirectToAction(nameof(Index));
                }

                // Không cho giữ bản sao còn lượt mượn chưa trả.
                bool dangMuon = await _db.Set<ChiTietMuon>()
                    .AnyAsync(x =>
                        x.BanSaoId == banSao.Id &&
                        x.NgayTra == null);

                if (dangMuon)
                {
                    TempData["Error"] =
                        "Bản sao chưa được trả hoàn tất.";
                    return RedirectToAction(nameof(Index));
                }

                dat.BanSaoGiuId = banSao.Id;
                dat.TrangThai = "DaGiu";

                // Quy định demo: giữ chỗ trong 3 ngày.
                dat.NgayHetHan = DateTime.Now.AddDays(3);

                banSao.TrangThai = "DaDat";

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Đã giữ sách cho độc giả trong 3 ngày.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể giữ bản sao do dữ liệu đã thay đổi.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================================
        // 6. HỦY ĐẶT TRƯỚC
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var dat = await _db.Set<DatTruocEntity>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dat == null)
                return NotFound();

            if (dat.TrangThai != "DangCho" &&
                dat.TrangThai != "DaGiu")
            {
                TempData["Error"] =
                    "Yêu cầu này không thể hủy.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu đã giữ bản sao, trả lại trạng thái sẵn sàng.
            if (dat.TrangThai == "DaGiu" &&
                dat.BanSaoGiuId.HasValue)
            {
                var bs = await _db.Set<BanSaoSach>()
                    .FindAsync(dat.BanSaoGiuId.Value);

                if (bs == null || bs.TrangThai != "DaDat")
                {
                    TempData["Error"] =
                        "Trạng thái bản sao không khớp. " +
                        "Cần kiểm tra trước khi hủy.";
                    return RedirectToAction(nameof(Index));
                }

                bs.TrangThai = "SanSang";
            }

            dat.TrangThai = "Huy";

            // Giữ lại BanSaoGiuId để tra cứu lịch sử.
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                "Đã hủy yêu cầu đặt trước.";

            return RedirectToAction(nameof(Index));
        }

        // ========================================
        // 7. XỬ LÝ YÊU CẦU GIỮ QUÁ HẠN
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Expire(int id)
        {
            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var dat = await _db.Set<DatTruocEntity>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dat == null)
                return NotFound();

            if (dat.TrangThai != "DaGiu" ||
                !dat.NgayHetHan.HasValue ||
                dat.NgayHetHan.Value > DateTime.Now)
            {
                TempData["Error"] =
                    "Yêu cầu chưa đến hạn xử lý.";
                return RedirectToAction(nameof(Index));
            }

            if (!dat.BanSaoGiuId.HasValue)
            {
                TempData["Error"] =
                    "Yêu cầu thiếu thông tin bản sao giữ.";
                return RedirectToAction(nameof(Index));
            }

            var bs = await _db.Set<BanSaoSach>()
                .FindAsync(dat.BanSaoGiuId.Value);

            if (bs == null || bs.TrangThai != "DaDat")
            {
                TempData["Error"] =
                    "Không thể giải phóng bản sao. " +
                    "Cần kiểm tra trạng thái thực tế.";
                return RedirectToAction(nameof(Index));
            }

            bs.TrangThai = "SanSang";
            dat.TrangThai = "QuaHan";

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                "Đã xử lý quá hạn và giải phóng bản sao.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GoiY(string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Array.Empty<object>());

            term = term.Trim();

            var data = await (
                from dt in _db.Set<DatTruocEntity>().AsNoTracking()
                join dg in _db.Set<DocGium>().AsNoTracking()
                    on dt.DocGiaId equals dg.Id
                join s in _db.Set<Sach>().AsNoTracking()
                    on dt.SachId equals s.Id
                where dg.MaDocGia.Contains(term)
                   || dg.HoTen.Contains(term)
                   || s.TieuDe.Contains(term)
                orderby dg.HoTen.StartsWith(term) descending,
                        dg.HoTen
                select new
                {
                    value = dg.MaDocGia,
                    label = dg.HoTen,
                    detail = dg.MaDocGia + " | " + s.TieuDe
                }
            ).Take(10).ToListAsync();

            return Json(data);
        }

    }
}
