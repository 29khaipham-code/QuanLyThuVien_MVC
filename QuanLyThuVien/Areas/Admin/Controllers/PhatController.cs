
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.Admin.Phat;

// Tránh trùng tên Phat giữa Entity và namespace
using PhatEntity = QuanLyThuVien.Models.Phat;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,ThuThu")]
    public class PhatController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        // Quy định phạt demo: 5.000 đồng/ngày
        private const decimal PhiQuaHanMoiNgay = 5000m;

        public PhatController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        // ========================================
        // 1. DANH SÁCH + TÌM KIẾM + LỌC
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
                from p in _db.Set<PhatEntity>().AsNoTracking()
                join ct in _db.Set<ChiTietMuon>().AsNoTracking()
                    on p.ChiTietMuonId equals ct.Id
                join pm in _db.Set<PhieuMuon>().AsNoTracking()
                    on ct.PhieuMuonId equals pm.Id
                join dg in _db.Set<DocGium>().AsNoTracking()
                    on pm.DocGiaId equals dg.Id
                join bs in _db.Set<BanSaoSach>().AsNoTracking()
                    on ct.BanSaoId equals bs.Id
                join s in _db.Set<Sach>().AsNoTracking()
                    on bs.SachId equals s.Id
                select new PhatListItemViewModel
                {
                    Id = p.Id,
                    ChiTietMuonId = p.ChiTietMuonId,

                    MaPhieu = pm.MaPhieu,
                    MaDocGia = dg.MaDocGia,
                    HoTen = dg.HoTen,
                    TenSach = s.TieuDe,
                    MaVach = bs.MaVach,

                    LoaiPhat = p.LoaiPhat,
                    LyDo = p.LyDo,
                    SoTien = p.SoTien,
                    DaThanhToan = p.DaThanhToan,
                    NgayThanhToan = p.NgayThanhToan,
                    NgayTao = p.NgayTao
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaPhieu.Contains(keyword) ||
                    x.MaDocGia.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    x.TenSach.Contains(keyword) ||
                    x.MaVach.Contains(keyword));
            }

            if (trangThai == "DaThanhToan")
            {
                query = query.Where(x => x.DaThanhToan);
            }
            else if (trangThai == "ChuaThanhToan")
            {
                query = query.Where(x => !x.DaThanhToan);
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

            // Tổng nợ trên toàn bộ database
            decimal tongNo = await _db.Set<PhatEntity>()
                .AsNoTracking()
                .Where(x => !x.DaThanhToan)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            // Tổng tiền đã thu trên toàn bộ database
            decimal daThu = await _db.Set<PhatEntity>()
                .AsNoTracking()
                .Where(x => x.DaThanhToan)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangThai;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;
            ViewBag.TongNo = tongNo;
            ViewBag.DaThu = daThu;

            return View(data);
        }

        // ========================================
        // 2. DANH SÁCH LƯỢT MƯỢN ĐỂ CHỌN
        // ========================================
        private async Task LoadOptions()
        {
            var ds = await (
                from ct in _db.Set<ChiTietMuon>().AsNoTracking()
                join pm in _db.Set<PhieuMuon>().AsNoTracking()
                    on ct.PhieuMuonId equals pm.Id
                join dg in _db.Set<DocGium>().AsNoTracking()
                    on pm.DocGiaId equals dg.Id
                join bs in _db.Set<BanSaoSach>().AsNoTracking()
                    on ct.BanSaoId equals bs.Id
                join s in _db.Set<Sach>().AsNoTracking()
                    on bs.SachId equals s.Id
                orderby ct.Id descending
                select new
                {
                    ct.Id,
                    HienThi = pm.MaPhieu + " | " +
                              dg.HoTen + " | " +
                              s.TieuDe + " | " +
                              bs.MaVach
                }
            ).Take(300).ToListAsync();

            ViewBag.ChiTietMuonOptions =
                new SelectList(ds, "Id", "HienThi");
        }

        // ========================================
        // 3. FORM TẠO PHẠT
        // ========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOptions();

            return View(new PhatFormViewModel
            {
                LoaiPhat = "TraMuon"
            });
        }

        // ========================================
        // 4. LƯU KHOẢN PHẠT
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PhatFormViewModel model)
        {
            bool loaiHopLe = model.LoaiPhat == "TraMuon"
                || model.LoaiPhat == "HuHong"
                || model.LoaiPhat == "MatSach";

            if (!loaiHopLe)
            {
                ModelState.AddModelError(
                    nameof(model.LoaiPhat),
                    "Loại phạt không hợp lệ.");
            }

            var ct = await _db.Set<ChiTietMuon>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == model.ChiTietMuonId);

            if (ct == null)
            {
                ModelState.AddModelError(
                    nameof(model.ChiTietMuonId),
                    "Lượt mượn không tồn tại.");
            }

            if (ct != null && model.LoaiPhat == "TraMuon")
            {
                if (ct.NgayTra == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Sách chưa được trả. " +
                        "Chưa thể chốt tiền phạt trả muộn.");
                }
                else
                {
                    DateOnly ngayTra =
                        DateOnly.FromDateTime(ct.NgayTra.Value);

                    int soNgayTre = Math.Max(
                        0,
                        ngayTra.DayNumber - ct.HanTra.DayNumber);

                    if (soNgayTre == 0)
                    {
                        ModelState.AddModelError(
                            "",
                            "Sách không trả muộn nên không phát sinh phạt.");
                    }
                    else
                    {
                        // Server tự tính, không tin giá trị từ form
                        model.SoTien =
                            soNgayTre * PhiQuaHanMoiNgay;

                        model.LyDo =
                            $"Trả muộn {soNgayTre} ngày, " +
                            $"mức phạt {PhiQuaHanMoiNgay:N0}đ/ngày.";
                    }
                }

                bool daPhatTraMuon = await _db.Set<PhatEntity>()
                    .AnyAsync(x =>
                        x.ChiTietMuonId == model.ChiTietMuonId &&
                        x.LoaiPhat == "TraMuon");

                if (daPhatTraMuon)
                {
                    ModelState.AddModelError(
                        "",
                        "Lượt mượn này đã có khoản phạt trả muộn.");
                }
            }

            if (model.LoaiPhat == "HuHong" ||
                model.LoaiPhat == "MatSach")
            {
                if (model.SoTien <= 0)
                {
                    ModelState.AddModelError(
                        nameof(model.SoTien),
                        "Số tiền phạt phải lớn hơn 0.");
                }

                if (string.IsNullOrWhiteSpace(model.LyDo))
                {
                    ModelState.AddModelError(
                        nameof(model.LyDo),
                        "Vui lòng nhập lý do phạt.");
                }

                // Tránh tạo cùng loại vi phạm nhiều lần
                bool daCoPhat = await _db.Set<PhatEntity>()
                    .AnyAsync(x =>
                        x.ChiTietMuonId == model.ChiTietMuonId &&
                        x.LoaiPhat == model.LoaiPhat);

                if (daCoPhat)
                {
                    ModelState.AddModelError(
                        "",
                        "Lượt mượn này đã có khoản phạt cùng loại.");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            var phat = new PhatEntity
            {
                ChiTietMuonId = model.ChiTietMuonId,
                LoaiPhat = model.LoaiPhat,
                LyDo = model.LyDo?.Trim(),
                SoTien = model.SoTien,
                DaThanhToan = false,
                NgayThanhToan = null,
                NgayTao = DateTime.Now
            };

            _db.Set<PhatEntity>().Add(phat);

            try
            {
                await _db.SaveChangesAsync();

                TempData["Success"] =
                    "Tạo khoản phạt thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không lưu được khoản phạt. " +
                    "Hãy kiểm tra dữ liệu hoặc khoản phạt trùng.");

                await LoadOptions();
                return View(model);
            }
        }

        // ========================================
        // 5. XÁC NHẬN ĐÃ THANH TOÁN
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            var phat = await _db.Set<PhatEntity>()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (phat == null)
                return NotFound();

            if (phat.DaThanhToan)
            {
                TempData["Error"] =
                    "Khoản phạt này đã được thanh toán.";

                return RedirectToAction(nameof(Index));
            }

            phat.DaThanhToan = true;
            phat.NgayThanhToan =
                DateOnly.FromDateTime(DateTime.Today);

            try
            {
                await _db.SaveChangesAsync();

                TempData["Success"] =
                    "Xác nhận thanh toán thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể cập nhật trạng thái thanh toán.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
