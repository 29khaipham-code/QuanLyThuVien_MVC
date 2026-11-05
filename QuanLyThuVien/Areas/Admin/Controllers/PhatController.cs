
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.Admin.Phat;
using System.Data;

using PhatEntity = QuanLyThuVien.Models.Phat;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,ThuThu")]
    public class PhatController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        // Mức phạt demo: 5.000 đồng / ngày trễ
        private const decimal PhiQuaHanMoiNgay = 5000m;
        private const int PageSize = 10;

        public PhatController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        // =========================================
        // 1. DANH SÁCH PHẠT
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword,
            string? trangThai,
            int page = 1)
        {
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

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaPhieu.Contains(keyword) ||
                    x.MaDocGia.Contains(keyword) ||
                    x.HoTen.Contains(keyword) ||
                    x.TenSach.Contains(keyword) ||
                    x.MaVach.Contains(keyword));
            }

            // Lọc trạng thái
            if (trangThai == "DaThanhToan")
            {
                query = query.Where(x => x.DaThanhToan);
            }
            else if (trangThai == "ChuaThanhToan")
            {
                query = query.Where(x => !x.DaThanhToan);
            }

            // Phân trang
            int total = await query.CountAsync();

            int totalPages = (int)Math.Ceiling(
                total / (double)PageSize);

            if (totalPages > 0)
                page = Math.Min(page, totalPages);

            var danhSach = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Tổng nợ toàn hệ thống
            decimal tongNo = await _db.Set<PhatEntity>()
                .AsNoTracking()
                .Where(x => !x.DaThanhToan)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            // Tổng đã thu toàn hệ thống
            decimal daThu = await _db.Set<PhatEntity>()
                .AsNoTracking()
                .Where(x => x.DaThanhToan)
                .SumAsync(x => (decimal?)x.SoTien) ?? 0m;

            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangThai ?? "";
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;
            ViewBag.TongNo = tongNo;
            ViewBag.DaThu = daThu;

            return View(danhSach);
        }

        // =========================================
        // 2. DANH SÁCH LƯỢT MƯỢN
        // =========================================
        private async Task LoadOptions()
        {
            var danhSach = await (
                from ct in _db.Set<ChiTietMuon>().AsNoTracking()
                join pm in _db.Set<PhieuMuon>().AsNoTracking()
                    on ct.PhieuMuonId equals pm.Id
                join dg in _db.Set<DocGium>().AsNoTracking()
                    on pm.DocGiaId equals dg.Id
                join bs in _db.Set<BanSaoSach>().AsNoTracking()
                    on ct.BanSaoId equals bs.Id
                join s in _db.Set<Sach>().AsNoTracking()
                    on bs.SachId equals s.Id
                where pm.TrangThai != "Huy"
                orderby ct.Id descending
                select new
                {
                    ct.Id,
                    HienThi =
                        pm.MaPhieu + " | " +
                        dg.HoTen + " | " +
                        s.TieuDe + " | " +
                        bs.MaVach
                }
            )
            .Take(300)
            .ToListAsync();

            ViewBag.ChiTietMuonOptions = new SelectList(
                danhSach,
                "Id",
                "HienThi");
        }

        // =========================================
        // 3. FORM TẠO KHOẢN PHẠT
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOptions();

            var model = new PhatFormViewModel
            {
                LoaiPhat = "TraMuon",
                SoTien = 0
            };

            return View(model);
        }

        // =========================================
        // 4. LƯU KHOẢN PHẠT
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PhatFormViewModel model)
        {
            // Kiểm tra loại phạt
            bool loaiHopLe =
                model.LoaiPhat == "TraMuon" ||
                model.LoaiPhat == "HuHong" ||
                model.LoaiPhat == "MatSach";

            if (!loaiHopLe)
            {
                ModelState.AddModelError(
                    nameof(model.LoaiPhat),
                    "Loại phạt không hợp lệ.");
            }

            // Kiểm tra lượt mượn
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

            // Không lập phạt cho phiếu đã hủy
            if (ct != null)
            {
                bool phieuHopLe = await _db.Set<PhieuMuon>()
                    .AnyAsync(x =>
                        x.Id == ct.PhieuMuonId &&
                        x.TrangThai != "Huy");

                if (!phieuHopLe)
                {
                    ModelState.AddModelError(
                        nameof(model.ChiTietMuonId),
                        "Phiếu mượn đã hủy hoặc không hợp lệ.");
                }
            }

            // =====================================
            // PHẠT TRẢ MUỘN
            // =====================================
            if (ct != null && model.LoaiPhat == "TraMuon")
            {
                if (!ct.NgayTra.HasValue)
                {
                    ModelState.AddModelError(
                        "",
                        "Sách chưa được trả. " +
                        "Không thể chốt tiền phạt trả muộn.");
                }
                else
                {
                    DateOnly ngayTra =
                        DateOnly.FromDateTime(
                            ct.NgayTra.Value);

                    int soNgayTre = Math.Max(
                        0,
                        ngayTra.DayNumber -
                        ct.HanTra.DayNumber);

                    if (soNgayTre == 0)
                    {
                        ModelState.AddModelError(
                            "",
                            "Sách trả đúng hạn hoặc sớm. " +
                            "Không phát sinh tiền phạt.");
                    }
                    else
                    {
                        // Tính trên server, không tin tiền từ form
                        model.SoTien =
                            soNgayTre * PhiQuaHanMoiNgay;

                        model.LyDo =
                            $"Trả muộn {soNgayTre} ngày, " +
                            $"phí {PhiQuaHanMoiNgay:N0}đ/ngày.";

                        // Cập nhật lại ModelState
                        ModelState.Remove(
                            nameof(model.SoTien));

                        ModelState.Remove(
                            nameof(model.LyDo));
                    }
                }
            }

            // =====================================
            // PHẠT HƯ HỎNG / MẤT SÁCH
            // =====================================
            if (model.LoaiPhat == "HuHong" ||
                model.LoaiPhat == "MatSach")
            {
                if (model.SoTien <= 0)
                {
                    ModelState.AddModelError(
                        nameof(model.SoTien),
                        "Tiền phạt phải lớn hơn 0.");
                }

                if (string.IsNullOrWhiteSpace(model.LyDo))
                {
                    ModelState.AddModelError(
                        nameof(model.LyDo),
                        "Vui lòng nhập lý do phạt.");
                }

                // Chỉ cho phép chốt hư hỏng/mất sách
                // khi lượt mượn đã được xử lý trả
                // hoặc ghi nhận tình trạng sau mượn.
                // Không dựa duy nhất vào trạng thái
                // bản sao vì có thể đã thay đổi sau đó.
                if (ct != null &&
                    ct.NgayTra == null &&
                    string.IsNullOrWhiteSpace(ct.TinhTrangSau))
                {
                    ModelState.AddModelError(
                        "",
                        "Lượt mượn chưa được xử lý. " +
                        "Hãy ghi nhận tình trạng sách " +
                        "trong nghiệp vụ mượn/trả trước.");
                }
            }

            // Nếu có lỗi thì trả lại form
            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            // =====================================
            // KIỂM TRA TRÙNG VÀ LƯU DB
            // =====================================
            try
            {
                // Serializable giúp tuần tự hóa kiểm tra
                // trùng và tạo khoản phạt.
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                bool daTonTai = await _db.Set<PhatEntity>()
                    .AnyAsync(x =>
                        x.ChiTietMuonId == model.ChiTietMuonId &&
                        x.LoaiPhat == model.LoaiPhat);

                if (daTonTai)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        "",
                        "Lượt mượn này đã có khoản phạt " +
                        "cùng loại. Không thể tạo trùng.");

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

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    "Tạo khoản phạt thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Không thể lưu khoản phạt. " +
                    "Có thể dữ liệu bị trùng hoặc " +
                    "không thỏa ràng buộc SQL.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "Có lỗi khi tạo khoản phạt. " +
                    "Vui lòng thử lại.");
            }

            await LoadOptions();
            return View(model);
        }

        // =========================================
        // 5. XÁC NHẬN THANH TOÁN
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            if (id <= 0)
                return BadRequest();

            var ngayThanhToan =
                DateOnly.FromDateTime(DateTime.Today);

            try
            {
                // Cập nhật trực tiếp có điều kiện:
                // Chỉ cập nhật nếu chưa thanh toán.
                int soDongCapNhat = await _db.Set<PhatEntity>()
                    .Where(x =>
                        x.Id == id &&
                        !x.DaThanhToan)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(
                            x => x.DaThanhToan,
                            true)
                        .SetProperty(
                            x => x.NgayThanhToan,
                            (DateOnly?)ngayThanhToan));

                if (soDongCapNhat == 1)
                {
                    TempData["Success"] =
                        "Xác nhận đã thu tiền phạt thành công!";
                }
                else
                {
                    bool tonTai = await _db.Set<PhatEntity>()
                        .AsNoTracking()
                        .AnyAsync(x => x.Id == id);

                    TempData["Error"] = tonTai
                        ? "Khoản phạt này đã được thanh toán."
                        : "Không tìm thấy khoản phạt.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể cập nhật thanh toán " +
                    "do lỗi dữ liệu.";
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Có lỗi khi xác nhận thanh toán.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
