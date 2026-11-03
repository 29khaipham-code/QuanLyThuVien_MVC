
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.Services;
using QuanLyThuVien.ViewModels.Admin.Sach;
using System.Data;

using SachEntity = QuanLyThuVien.Models.Sach;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,ThuThu")]
    public class SachAdminController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        private readonly SachCoverService _covers;

        public SachAdminController(
            QuanLyThuVienContext db,
            SachCoverService covers)
        {
            _db = db;
            _covers = covers;
        }

        // ==========================
        // DANH SÁCH SÁCH
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword, int page = 1)
        {
            const int pageSize = 10;
            page = Math.Max(1, page);
            keyword = keyword?.Trim();

            var query =
                from s in _db.Set<SachEntity>().AsNoTracking()
                join tl in _db.Set<TheLoai>().AsNoTracking()
                    on s.TheLoaiId equals tl.Id
                join nxb in _db.Set<NhaXuatBan>().AsNoTracking()
                    on s.NhaXuatBanId equals nxb.Id
                select new SachListItemViewModel
                {
                    Id = s.Id,
                    Isbn = s.Isbn,
                    TieuDe = s.TieuDe,
                    TheLoai = tl.TenTheLoai,
                    NhaXuatBan = nxb.TenNhaXuatBan,
                    NamXuatBan = s.NamXuatBan,
                    AnhBiaUrl = s.AnhBiaUrl,
                    TrangThai = s.TrangThai
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.TieuDe.Contains(keyword) ||
                    (x.Isbn != null && x.Isbn.Contains(keyword)));
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

        // ==========================
        // CÁC HÀM HỖ TRỢ
        // ==========================
        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static List<string> TachTacGia(string? text)
        {
            return (text ?? "")
                .Split(';',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task ValidateForm(
            SachFormViewModel model,
            int? excludeId = null)
        {
            model.TenTheLoai = model.TenTheLoai?.Trim() ?? "";
            model.TenNhaXuatBan =
                model.TenNhaXuatBan?.Trim() ?? "";
            model.TenTacGia = model.TenTacGia?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(model.TenTheLoai))
                ModelState.AddModelError(
                    nameof(model.TenTheLoai),
                    "Phải nhập thể loại.");

            if (model.TenTheLoai.Length > 100)
                ModelState.AddModelError(
                    nameof(model.TenTheLoai),
                    "Tên thể loại tối đa 100 ký tự.");

            if (string.IsNullOrWhiteSpace(model.TenNhaXuatBan))
                ModelState.AddModelError(
                    nameof(model.TenNhaXuatBan),
                    "Phải nhập nhà xuất bản.");

            if (model.TenNhaXuatBan.Length > 150)
                ModelState.AddModelError(
                    nameof(model.TenNhaXuatBan),
                    "Tên NXB tối đa 150 ký tự.");

            var authors = TachTacGia(model.TenTacGia);

            if (authors.Count == 0)
                ModelState.AddModelError(
                    nameof(model.TenTacGia),
                    "Phải nhập ít nhất một tác giả.");

            if (authors.Any(x => x.Length > 150))
                ModelState.AddModelError(
                    nameof(model.TenTacGia),
                    "Tên mỗi tác giả tối đa 150 ký tự.");

            if (model.TrangThai != "HoatDong" &&
                model.TrangThai != "NgungPhucVu")
                ModelState.AddModelError(
                    nameof(model.TrangThai),
                    "Trạng thái không hợp lệ.");

            string? isbn = Normalize(model.Isbn);

            if (isbn != null)
            {
                bool trung = await _db.Set<SachEntity>()
                    .AnyAsync(x =>
                        x.Isbn == isbn &&
                        (!excludeId.HasValue ||
                         x.Id != excludeId.Value));

                if (trung)
                    ModelState.AddModelError(
                        nameof(model.Isbn),
                        "ISBN đã tồn tại.");
            }

            if (model.AnhBiaFile != null &&
                model.AnhBiaFile.Length > 2 * 1024 * 1024)
                ModelState.AddModelError(
                    nameof(model.AnhBiaFile),
                    "Ảnh tối đa 2 MB.");
        }

        // ==========================
        // TÌM HOẶC THÊM THỂ LOẠI
        // ==========================
        private async Task<int> GetOrCreateTheLoaiAsync(
            string ten)
        {
            ten = ten.Trim();

            var item = await _db.Set<TheLoai>()
                .FirstOrDefaultAsync(x => x.TenTheLoai == ten);

            if (item != null)
                return item.Id;

            item = new TheLoai
            {
                TenTheLoai = ten
            };

            _db.Set<TheLoai>().Add(item);
            await _db.SaveChangesAsync();

            return item.Id;
        }

        // ==========================
        // TÌM HOẶC THÊM NXB
        // ==========================
        private async Task<int> GetOrCreateNhaXuatBanAsync(
            string ten)
        {
            ten = ten.Trim();

            var item = await _db.Set<NhaXuatBan>()
                .FirstOrDefaultAsync(
                    x => x.TenNhaXuatBan == ten);

            if (item != null)
                return item.Id;

            item = new NhaXuatBan
            {
                TenNhaXuatBan = ten
            };

            _db.Set<NhaXuatBan>().Add(item);
            await _db.SaveChangesAsync();

            return item.Id;
        }

        // ==========================
        // TÌM HOẶC THÊM TÁC GIẢ
        // ==========================
        private async Task<List<int>> GetOrCreateTacGiaAsync(
            string text)
        {
            var ids = new List<int>();

            foreach (string ten in TachTacGia(text))
            {
                var item = await _db.Set<TacGium>()
                    .FirstOrDefaultAsync(
                        x => x.TenTacGia == ten);

                if (item == null)
                {
                    item = new TacGium
                    {
                        TenTacGia = ten
                    };

                    _db.Set<TacGium>().Add(item);
                    await _db.SaveChangesAsync();
                }

                ids.Add(item.Id);
            }

            return ids.Distinct().ToList();
        }

        // ==========================
        // THÊM SÁCH
        // ==========================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SachFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            SachFormViewModel model)
        {
            await ValidateForm(model);

            if (!ModelState.IsValid)
                return View(model);

            string? anhMoi = null;

            try
            {
                if (model.AnhBiaFile != null)
                    anhMoi = await _covers.SaveAsync(
                        model.AnhBiaFile);
            }
            catch (Exception ex) when (
                ex is InvalidOperationException or IOException)
            {
                ModelState.AddModelError(
                    nameof(model.AnhBiaFile),
                    ex.Message);

                return View(model);
            }

            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                int theLoaiId =
                    await GetOrCreateTheLoaiAsync(
                        model.TenTheLoai);

                int nhaXuatBanId =
                    await GetOrCreateNhaXuatBanAsync(
                        model.TenNhaXuatBan);

                List<int> tacGiaIds =
                    await GetOrCreateTacGiaAsync(
                        model.TenTacGia);

                var sach = new SachEntity
                {
                    Isbn = Normalize(model.Isbn),
                    TieuDe = model.TieuDe.Trim(),
                    TheLoaiId = theLoaiId,
                    NhaXuatBanId = nhaXuatBanId,
                    NamXuatBan = model.NamXuatBan,
                    LanTaiBan = model.LanTaiBan,
                    MoTa = Normalize(model.MoTa),
                    AnhBiaUrl = anhMoi,
                    TrangThai = model.TrangThai,
                    NgayTao = DateTime.Now
                };

                _db.Set<SachEntity>().Add(sach);

                await _db.SaveChangesAsync();

                foreach (int tacGiaId in tacGiaIds)
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
            catch (Exception)
            {
                TryDeleteCover(anhMoi);

                ModelState.AddModelError(
                    "",
                    "Không thể lưu sách. Kiểm tra ISBN " +
                    "và thông tin nhập.");
            }

            return View(model);
        }

        // ==========================
        // SỬA SÁCH
        // ==========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sach = await _db.Set<SachEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sach == null)
                return NotFound();

            string tenTheLoai = await _db.Set<TheLoai>()
                .AsNoTracking()
                .Where(x => x.Id == sach.TheLoaiId)
                .Select(x => x.TenTheLoai)
                .FirstOrDefaultAsync() ?? "";

            string tenNxb = await _db.Set<NhaXuatBan>()
                .AsNoTracking()
                .Where(x => x.Id == sach.NhaXuatBanId)
                .Select(x => x.TenNhaXuatBan)
                .FirstOrDefaultAsync() ?? "";

            var authors = await _db.Database
                .SqlQueryRaw<string>(
                    @"SELECT tg.TenTacGia AS Value
                      FROM dbo.SachTacGia st
                      JOIN dbo.TacGia tg
                        ON tg.Id = st.TacGiaId
                      WHERE st.SachId = {0}", id)
                .ToListAsync();

            var model = new SachFormViewModel
            {
                Id = sach.Id,
                Isbn = sach.Isbn,
                TieuDe = sach.TieuDe,
                TenTheLoai = tenTheLoai,
                TenNhaXuatBan = tenNxb,
                TenTacGia = string.Join("; ", authors),
                NamXuatBan = sach.NamXuatBan,
                LanTaiBan = sach.LanTaiBan,
                MoTa = sach.MoTa,
                AnhBiaUrl = sach.AnhBiaUrl,
                TrangThai = sach.TrangThai
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SachFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            var sach = await _db.Set<SachEntity>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sach == null)
                return NotFound();

            await ValidateForm(model, id);

            string? anhCu = sach.AnhBiaUrl;

            if (!ModelState.IsValid)
            {
                model.AnhBiaUrl = anhCu;
                return View(model);
            }

            string? anhMoi = null;

            try
            {
                if (model.AnhBiaFile != null)
                    anhMoi = await _covers.SaveAsync(
                        model.AnhBiaFile);
            }
            catch (Exception ex) when (
                ex is InvalidOperationException or IOException)
            {
                ModelState.AddModelError(
                    nameof(model.AnhBiaFile),
                    ex.Message);

                model.AnhBiaUrl = anhCu;
                return View(model);
            }

            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                int theLoaiId =
                    await GetOrCreateTheLoaiAsync(
                        model.TenTheLoai);

                int nhaXuatBanId =
                    await GetOrCreateNhaXuatBanAsync(
                        model.TenNhaXuatBan);

                List<int> tacGiaIds =
                    await GetOrCreateTacGiaAsync(
                        model.TenTacGia);

                sach.Isbn = Normalize(model.Isbn);
                sach.TieuDe = model.TieuDe.Trim();
                sach.TheLoaiId = theLoaiId;
                sach.NhaXuatBanId = nhaXuatBanId;
                sach.NamXuatBan = model.NamXuatBan;
                sach.LanTaiBan = model.LanTaiBan;
                sach.MoTa = Normalize(model.MoTa);
                sach.TrangThai = model.TrangThai;
                sach.NgayCapNhat = DateTime.Now;

                if (anhMoi != null)
                    sach.AnhBiaUrl = anhMoi;

                await _db.SaveChangesAsync();

                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"DELETE FROM dbo.SachTacGia
                       WHERE SachId = {id}");

                foreach (int tacGiaId in tacGiaIds)
                {
                    await _db.Database.ExecuteSqlInterpolatedAsync(
                        $@"INSERT INTO dbo.SachTacGia
                           (SachId, TacGiaId)
                           VALUES ({id}, {tacGiaId})");
                }

                await transaction.CommitAsync();

                if (anhMoi != null)
                    TryDeleteCover(anhCu);

                TempData["Success"] =
                    "Cập nhật sách thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TryDeleteCover(anhMoi);

                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật sách.");
            }

            model.AnhBiaUrl = anhCu;
            return View(model);
        }

        // ==========================
        // NGỪNG PHỤC VỤ
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(int id)
        {
            var sach = await _db.Set<SachEntity>()
                .FindAsync(id);

            if (sach == null)
                return NotFound();

            sach.TrangThai = "NgungPhucVu";
            sach.NgayCapNhat = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["Success"] =
                "Đã ngừng phục vụ sách.";

            return RedirectToAction(nameof(Index));
        }

        private void TryDeleteCover(string? url)
        {
            try
            {
                _covers.DeleteIfLocal(url);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sach = await _db.Set<SachEntity>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sach == null)
            {
                TempData["Error"] = "Không tìm thấy sách cần xóa.";
                return RedirectToAction(nameof(Index));
            }

            // Không xóa nếu sách đã có bản sao vật lý
            bool coBanSao = await _db.Set<BanSaoSach>()
                .AnyAsync(x => x.SachId == id);

            if (coBanSao)
            {
                TempData["Error"] =
                    "Không thể xóa vì sách đã có bản sao vật lý. " +
                    "Hãy dùng chức năng Ngừng phục vụ.";

                return RedirectToAction(nameof(Index));
            }

            // Không xóa nếu sách có tệp điện tử
            bool coTepDienTu = await _db.Set<TepSachDienTu>()
                .AnyAsync(x => x.SachId == id);

            if (coTepDienTu)
            {
                TempData["Error"] =
                    "Không thể xóa vì sách có tệp điện tử liên kết.";

                return RedirectToAction(nameof(Index));
            }

            // Không xóa nếu sách đã có lịch sử đặt trước
            bool coDatTruoc = await _db.Set<DatTruoc>()
                .AnyAsync(x => x.SachId == id);

            if (coDatTruoc)
            {
                TempData["Error"] =
                    "Không thể xóa vì sách đã có lịch sử đặt trước.";

                return RedirectToAction(nameof(Index));
            }

            string? anhBiaCu = sach.AnhBiaUrl;

            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync();

                // Xóa quan hệ nhiều-nhiều với tác giả
                await _db.Database.ExecuteSqlInterpolatedAsync(
                    $@"DELETE FROM dbo.SachTacGia
               WHERE SachId = {id}");

                // Xóa đầu sách
                _db.Set<SachEntity>().Remove(sach);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa sách vì vẫn có dữ liệu liên quan.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Đã xảy ra lỗi khi xóa sách.";

                return RedirectToAction(nameof(Index));
            }

            // Chỉ dọn ảnh sau khi SQL xóa thành công
            try
            {
                _covers.DeleteIfLocal(anhBiaCu);
            }
            catch (IOException)
            {
                // Sách đã xóa trong DB, không làm hỏng kết quả.
            }
            catch (UnauthorizedAccessException)
            {
                // Không đủ quyền xóa file ảnh.
            }

            TempData["Success"] = "Xóa sách thành công!";

            return RedirectToAction(nameof(Index));
        }

    }
}
