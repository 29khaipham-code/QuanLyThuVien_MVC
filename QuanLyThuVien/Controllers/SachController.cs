
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.Sach;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class SachController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        public SachController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        // Danh sách, tìm kiếm, phân trang
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword, int page = 1)
        {
            const int pageSize = 10;
            page = Math.Max(1, page);
            keyword = keyword?.Trim();

            var query =
                from s in _db.Set<Sach>().AsNoTracking()
                join tl in _db.Set<TheLoai>()
                    on s.TheLoaiId equals tl.Id
                join nxb in _db.Set<NhaXuatBan>()
                    on s.NhaXuatBanId equals nxb.Id
                select new SachListItemViewModel
                {
                    Id = s.Id,
                    Isbn = s.Isbn,
                    TieuDe = s.TieuDe,
                    TheLoai = tl.TenTheLoai,
                    NhaXuatBan = nxb.TenNhaXuatBan,
                    NamXuatBan = s.NamXuatBan,
                    TrangThai = s.TrangThai
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.TieuDe.Contains(keyword) ||
                    (x.Isbn != null &&
                     x.Isbn.Contains(keyword)));
            }

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(
                total / (double)pageSize);

            if (totalPages > 0)
                page = Math.Min(page, totalPages);

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            return View(data);
        }

        // Danh sách chọn Thể loại, NXB, Tác giả
        private async Task LoadOptions()
        {
            ViewBag.TheLoaiOptions = new SelectList(
                await _db.Set<TheLoai>()
                    .AsNoTracking()
                    .OrderBy(x => x.TenTheLoai)
                    .ToListAsync(),
                "Id", "TenTheLoai");

            ViewBag.NhaXuatBanOptions = new SelectList(
                await _db.Set<NhaXuatBan>()
                    .AsNoTracking()
                    .OrderBy(x => x.TenNhaXuatBan)
                    .ToListAsync(),
                "Id", "TenNhaXuatBan");

            ViewBag.TacGiaOptions = new SelectList(
                await _db.Set<TacGium>()
                    .AsNoTracking()
                    .OrderBy(x => x.TenTacGia)
                    .ToListAsync(),
                "Id", "TenTacGia");
        }

        // Form thêm
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOptions();
            return View(new SachFormViewModel());
        }

        // Lưu sách mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SachFormViewModel model)
        {
            await ValidateForm(model);

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            await using var transaction =
                await _db.Database.BeginTransactionAsync();

            var sach = new Sach
            {
                Isbn = Normalize(model.Isbn),
                TieuDe = model.TieuDe.Trim(),
                TheLoaiId = model.TheLoaiId,
                NhaXuatBanId = model.NhaXuatBanId,
                NamXuatBan = model.NamXuatBan,
                LanTaiBan = model.LanTaiBan,
                MoTa = Normalize(model.MoTa),
                AnhBiaUrl = Normalize(model.AnhBiaUrl),
                TrangThai = model.TrangThai,
                NgayTao = DateTime.Now
            };

            _db.Set<Sach>().Add(sach);
            await _db.SaveChangesAsync();

            foreach (int tacGiaId in model.TacGiaIds.Distinct())
            {
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"INSERT INTO dbo.SachTacGia
                       (SachId, TacGiaId)
                       VALUES ({sach.Id}, {tacGiaId})");
            }

            await transaction.CommitAsync();

            TempData["Success"] = "Thêm sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Form sửa
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sach = await _db.Set<Sach>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sach == null)
                return NotFound();

            var ids = await _db.Database
                .SqlQueryRaw<int>(
                    @"SELECT TacGiaId AS Value
                      FROM dbo.SachTacGia
                      WHERE SachId = {0}", id)
                .ToListAsync();

            var model = new SachFormViewModel
            {
                Id = sach.Id,
                Isbn = sach.Isbn,
                TieuDe = sach.TieuDe,
                TheLoaiId = sach.TheLoaiId,
                NhaXuatBanId = sach.NhaXuatBanId,
                TacGiaIds = ids,
                NamXuatBan = sach.NamXuatBan,
                LanTaiBan = sach.LanTaiBan,
                MoTa = sach.MoTa,
                AnhBiaUrl = sach.AnhBiaUrl,
                TrangThai = sach.TrangThai
            };

            await LoadOptions();
            return View(model);
        }

        // Lưu thông tin sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, SachFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            await ValidateForm(model, id);

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            var sach = await _db.Set<Sach>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sach == null)
                return NotFound();

            await using var transaction =
                await _db.Database.BeginTransactionAsync();

            sach.Isbn = Normalize(model.Isbn);
            sach.TieuDe = model.TieuDe.Trim();
            sach.TheLoaiId = model.TheLoaiId;
            sach.NhaXuatBanId = model.NhaXuatBanId;
            sach.NamXuatBan = model.NamXuatBan;
            sach.LanTaiBan = model.LanTaiBan;
            sach.MoTa = Normalize(model.MoTa);
            sach.AnhBiaUrl = Normalize(model.AnhBiaUrl);
            sach.TrangThai = model.TrangThai;
            sach.NgayCapNhat = DateTime.Now;

            await _db.SaveChangesAsync();

            // Cập nhật bảng liên kết tác giả
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $@"DELETE FROM dbo.SachTacGia
                   WHERE SachId = {id}");

            foreach (int tacGiaId in model.TacGiaIds.Distinct())
            {
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"INSERT INTO dbo.SachTacGia
                       (SachId, TacGiaId)
                       VALUES ({id}, {tacGiaId})");
            }

            await transaction.CommitAsync();

            TempData["Success"] = "Cập nhật sách thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Ngừng phục vụ thay vì xóa dữ liệu lịch sử
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(int id)
        {
            var sach = await _db.Set<Sach>().FindAsync(id);

            if (sach == null)
                return NotFound();

            sach.TrangThai = "NgungPhucVu";
            sach.NgayCapNhat = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Đã chuyển sách sang trạng thái ngừng phục vụ.";

            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra dữ liệu trước khi lưu
        private async Task ValidateForm(
            SachFormViewModel model, int? excludeId = null)
        {
            string? isbn = Normalize(model.Isbn);

            if (!string.IsNullOrEmpty(isbn))
            {
                bool duplicate = await _db.Set<Sach>()
                    .AnyAsync(x =>
                        x.Isbn == isbn &&
                        (!excludeId.HasValue ||
                         x.Id != excludeId.Value));

                if (duplicate)
                    ModelState.AddModelError(
                        nameof(model.Isbn),
                        "ISBN đã tồn tại.");
            }

            bool validCategory = await _db.Set<TheLoai>()
                .AnyAsync(x => x.Id == model.TheLoaiId);

            if (!validCategory)
                ModelState.AddModelError(
                    nameof(model.TheLoaiId),
                    "Thể loại không tồn tại.");

            bool validPublisher = await _db.Set<NhaXuatBan>()
                .AnyAsync(x => x.Id == model.NhaXuatBanId);

            if (!validPublisher)
                ModelState.AddModelError(
                    nameof(model.NhaXuatBanId),
                    "Nhà xuất bản không tồn tại.");

            var authorIds = model.TacGiaIds?
                .Distinct().ToList() ?? new List<int>();

            model.TacGiaIds = authorIds;

            int validAuthors = await _db.Set<TacGium>()
                .CountAsync(x => authorIds.Contains(x.Id));

            if (validAuthors != authorIds.Count)
                ModelState.AddModelError(
                    nameof(model.TacGiaIds),
                    "Danh sách tác giả không hợp lệ.");

            if (model.TrangThai != "HoatDong" &&
                model.TrangThai != "NgungPhucVu")
                ModelState.AddModelError(
                    nameof(model.TrangThai),
                    "Trạng thái không hợp lệ.");
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
