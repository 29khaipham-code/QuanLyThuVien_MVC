
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.DocGia;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class DocGiaController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        public DocGiaController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        // 1. Danh sách + tìm kiếm + phân trang
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword, int page = 1)
        {
            const int pageSize = 10;
            page = Math.Max(page, 1);
            keyword = keyword?.Trim();

            var query =
                from dg in _db.Set<DocGium>().AsNoTracking()
                join loai in _db.Set<LoaiDocGium>().AsNoTracking()
                    on dg.LoaiDocGiaId equals loai.Id
                select new DocGiaListItemViewModel
                {
                    Id = dg.Id,
                    MaDocGia = dg.MaDocGia,
                    HoTen = dg.HoTen,
                    Email = dg.Email,
                    SoDienThoai = dg.SoDienThoai,
                    TenLoai = loai.TenLoai,
                    NgayHetHan = dg.NgayHetHan,
                    TrangThai = dg.TrangThai,
                    TaiKhoanId = dg.TaiKhoanId
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaDocGia.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    (x.Email != null && x.Email.Contains(keyword)) ||
                    (x.SoDienThoai != null &&
                     x.SoDienThoai.Contains(keyword)));
            }

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(
                total / (double)pageSize);

            if (totalPages > 0)
                page = Math.Min(page, totalPages);

            var data = await query
                .OrderBy(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            return View(data);
        }

        // 2. Tải dữ liệu ComboBox
        private async Task LoadOptions()
        {
            var loaiDocGia = await _db.Set<LoaiDocGium>()
                .AsNoTracking()
                .OrderBy(x => x.TenLoai)
                .ToListAsync();

            ViewBag.LoaiDocGiaOptions = new SelectList(
                loaiDocGia, "Id", "TenLoai");

            // Chỉ hiện tài khoản mang vai trò DocGia
            var taiKhoan = await _db.Set<TaiKhoan>()
                .AsNoTracking()
                .Where(x => x.VaiTro.TenVaiTro == "DocGia")
                .OrderBy(x => x.TenDangNhap)
                .Select(x => new
                {
                    x.Id,
                    HienThi = x.TenDangNhap + " - " + x.HoTen
                })
                .ToListAsync();

            ViewBag.TaiKhoanOptions = new SelectList(
                taiKhoan, "Id", "HienThi");
        }

        // 3. Kiểm tra dữ liệu form
        private async Task ValidateForm(
            DocGiaFormViewModel model,
            int? excludeId = null)
        {
            model.MaDocGia = model.MaDocGia?.Trim() ?? "";
            model.HoTen = model.HoTen?.Trim() ?? "";
            model.Email = Normalize(model.Email);
            model.SoDienThoai = Normalize(model.SoDienThoai);
            model.DiaChi = Normalize(model.DiaChi);

            // Kiểm tra mã độc giả trùng
            bool trungMa = await _db.Set<DocGium>()
                .AnyAsync(x =>
                    x.MaDocGia == model.MaDocGia &&
                    (!excludeId.HasValue ||
                     x.Id != excludeId.Value));

            if (trungMa)
                ModelState.AddModelError(
                    nameof(model.MaDocGia),
                    "Mã độc giả đã tồn tại.");

            // Kiểm tra loại độc giả
            bool loaiHopLe = await _db.Set<LoaiDocGium>()
                .AnyAsync(x => x.Id == model.LoaiDocGiaId);

            if (!loaiHopLe)
                ModelState.AddModelError(
                    nameof(model.LoaiDocGiaId),
                    "Loại độc giả không tồn tại.");

            // Kiểm tra ngày đăng ký và hết hạn
            if (model.NgayHetHan < model.NgayDangKy)
                ModelState.AddModelError(
                    nameof(model.NgayHetHan),
                    "Ngày hết hạn không được trước ngày đăng ký.");

            if (model.NgaySinh.HasValue &&
                model.NgaySinh.Value > DateOnly.FromDateTime(DateTime.Today))
                ModelState.AddModelError(
                    nameof(model.NgaySinh),
                    "Ngày sinh không được ở tương lai.");

            if (model.GioiTinh?.Length > 10)
                ModelState.AddModelError(
                    nameof(model.GioiTinh),
                    "Giới tính tối đa 10 ký tự.");

            if (!new[] { "HoatDong", "TamKhoa", "HetHan" }
                .Contains(model.TrangThai))
                ModelState.AddModelError(
                    nameof(model.TrangThai),
                    "Trạng thái không hợp lệ.");

            // Một tài khoản chỉ thuộc một độc giả
            if (model.TaiKhoanId.HasValue)
            {
                int tkId = model.TaiKhoanId.Value;

                bool tkHopLe = await _db.Set<TaiKhoan>()
                    .AnyAsync(x =>
                        x.Id == tkId &&
                        x.VaiTro.TenVaiTro == "DocGia");

                bool daGan = await _db.Set<DocGium>()
                    .AnyAsync(x =>
                        x.TaiKhoanId == tkId &&
                        (!excludeId.HasValue ||
                         x.Id != excludeId.Value));

                if (!tkHopLe || daGan)
                    ModelState.AddModelError(
                        nameof(model.TaiKhoanId),
                        "Tài khoản không hợp lệ hoặc đã liên kết với độc giả khác.");
            }
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        // 4. Form thêm
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOptions();
            return View(new DocGiaFormViewModel());
        }

        // 5. Lưu độc giả mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            DocGiaFormViewModel model)
        {
            if (ModelState.IsValid)
                await ValidateForm(model);

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            var dg = new DocGium
            {
                MaDocGia = model.MaDocGia,
                HoTen = model.HoTen,
                NgaySinh = model.NgaySinh,
                GioiTinh = Normalize(model.GioiTinh),
                Email = model.Email,
                SoDienThoai = model.SoDienThoai,
                DiaChi = model.DiaChi,
                NgayDangKy = model.NgayDangKy,
                NgayHetHan = model.NgayHetHan,
                TrangThai = model.TrangThai,
                LoaiDocGiaId = model.LoaiDocGiaId,
                TaiKhoanId = model.TaiKhoanId
            };

            _db.Set<DocGium>().Add(dg);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể thêm độc giả. Kiểm tra mã độc giả hoặc tài khoản liên kết.");

                await LoadOptions();
                return View(model);
            }

            TempData["Success"] = "Thêm độc giả thành công!";
            return RedirectToAction(nameof(Index));
        }

        // 6. Form sửa
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dg = await _db.Set<DocGium>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (dg == null)
                return NotFound();

            var model = new DocGiaFormViewModel
            {
                Id = dg.Id,
                MaDocGia = dg.MaDocGia,
                HoTen = dg.HoTen,
                NgaySinh = dg.NgaySinh,
                GioiTinh = dg.GioiTinh,
                Email = dg.Email,
                SoDienThoai = dg.SoDienThoai,
                DiaChi = dg.DiaChi,
                NgayDangKy = dg.NgayDangKy,
                NgayHetHan = dg.NgayHetHan,
                TrangThai = dg.TrangThai,
                LoaiDocGiaId = dg.LoaiDocGiaId,
                TaiKhoanId = dg.TaiKhoanId
            };

            await LoadOptions();
            return View(model);
        }

        // 7. Lưu chỉnh sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, DocGiaFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
                await ValidateForm(model, id);

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            var dg = await _db.Set<DocGium>().FindAsync(id);

            if (dg == null)
                return NotFound();

            dg.MaDocGia = model.MaDocGia;
            dg.HoTen = model.HoTen;
            dg.NgaySinh = model.NgaySinh;
            dg.GioiTinh = Normalize(model.GioiTinh);
            dg.Email = model.Email;
            dg.SoDienThoai = model.SoDienThoai;
            dg.DiaChi = model.DiaChi;
            dg.NgayDangKy = model.NgayDangKy;
            dg.NgayHetHan = model.NgayHetHan;
            dg.TrangThai = model.TrangThai;
            dg.LoaiDocGiaId = model.LoaiDocGiaId;
            dg.TaiKhoanId = model.TaiKhoanId;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật. Kiểm tra dữ liệu và tài khoản liên kết.");

                await LoadOptions();
                return View(model);
            }

            TempData["Success"] = "Cập nhật độc giả thành công!";
            return RedirectToAction(nameof(Index));
        }

        // 8. Khóa / mở khóa thẻ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var dg = await _db.Set<DocGium>().FindAsync(id);

            if (dg == null)
                return NotFound();

            if (dg.TrangThai == "TamKhoa")
            {
                dg.TrangThai =
                    dg.NgayHetHan >= DateOnly.FromDateTime(DateTime.Today)
                    ? "HoatDong"
                    : "HetHan";
            }
            else
            {
                dg.TrangThai = "TamKhoa";
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật trạng thái thẻ thành công.";
            return RedirectToAction(nameof(Index));
        }

        // 9. Xóa độc giả nếu chưa có dữ liệu liên quan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dg = await _db.Set<DocGium>().FindAsync(id);

            if (dg == null)
                return NotFound();

            bool daMuon = await _db.Set<PhieuMuon>()
                .AnyAsync(x => x.DocGiaId == id);

            if (daMuon)
            {
                TempData["Error"] =
                    "Độc giả đã có lịch sử mượn, không thể xóa.";
                return RedirectToAction(nameof(Index));
            }

            _db.Set<DocGium>().Remove(dg);

            try
            {
                await _db.SaveChangesAsync();
                TempData["Success"] = "Xóa độc giả thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa vì độc giả đã có dữ liệu liên quan (đặt trước, đọc sách điện tử...).";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
