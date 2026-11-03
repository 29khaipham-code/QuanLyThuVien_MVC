
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.Admin.MuonTra;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,ThuThu")]
    public class MuonTraController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        public MuonTraController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        // ============================================
        // 1. DANH SÁCH PHIẾU MƯỢN
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? keyword, int page = 1)
        {
            const int pageSize = 10;
            page = Math.Max(page, 1);
            keyword = keyword?.Trim();

            var query =
                from pm in _db.Set<PhieuMuon>().AsNoTracking()
                join dg in _db.Set<DocGium>().AsNoTracking()
                    on pm.DocGiaId equals dg.Id
                select new PhieuMuonListItemViewModel
                {
                    Id = pm.Id,
                    MaPhieu = pm.MaPhieu,
                    TenDocGia = dg.HoTen,
                    NgayMuon = pm.NgayMuon,
                    TrangThai = pm.TrangThai,

                    SoCuon = _db.Set<ChiTietMuon>()
                        .Count(ct => ct.PhieuMuonId == pm.Id),

                    ChuaTra = _db.Set<ChiTietMuon>()
                        .Count(ct =>
                            ct.PhieuMuonId == pm.Id &&
                            ct.NgayTra == null)
                };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaPhieu.Contains(keyword) ||
                    x.TenDocGia.Contains(keyword));
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

        // ============================================
        // 2. DỮ LIỆU CHO FORM TẠO PHIẾU
        // ============================================
        private async Task LoadOptions()
        {
            var docGias = await _db.Set<DocGium>()
                .AsNoTracking()
                .OrderBy(x => x.HoTen)
                .Select(x => new
                {
                    x.Id,
                    HienThi = x.MaDocGia + " - " + x.HoTen
                })
                .ToListAsync();

            ViewBag.DocGiaOptions = new SelectList(
                docGias, "Id", "HienThi");

            var banSaos = await (
                from bs in _db.Set<BanSaoSach>().AsNoTracking()
                join s in _db.Set<Sach>().AsNoTracking()
                    on bs.SachId equals s.Id

                where bs.TrangThai == "SanSang"
                    && s.TrangThai == "HoatDong"
                    && !_db.Set<ChiTietMuon>().Any(ct =>
                        ct.BanSaoId == bs.Id &&
                        ct.NgayTra == null)

                orderby s.TieuDe, bs.MaVach

                select new
                {
                    bs.Id,
                    HienThi = bs.MaVach + " - " + s.TieuDe
                }
            ).ToListAsync();

            ViewBag.BanSaoOptions = new SelectList(
                banSaos, "Id", "HienThi");
        }

        // ============================================
        // 3. FORM TẠO PHIẾU MƯỢN
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadOptions();

            return View(new TaoPhieuMuonViewModel());
        }

        // ============================================
        // 4. LƯU PHIẾU MƯỢN
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TaoPhieuMuonViewModel model)
        {
            model.BanSaoIds ??= new List<int>();

            var ids = model.BanSaoIds
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.BanSaoIds),
                    "Vui lòng chọn ít nhất một bản sao sách.");
            }

            if (ids.Any(x => x <= 0))
            {
                ModelState.AddModelError(
                    nameof(model.BanSaoIds),
                    "Danh sách bản sao không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return View(model);
            }

            var userIdText = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdText, out int nhanVienId))
                return Forbid();

            var nhanVienHopLe = await _db.Set<TaiKhoan>()
                .AnyAsync(x => x.Id == nhanVienId
                    && x.HoatDong);

            if (!nhanVienHopLe)
                return Forbid();

            string? loi = null;
            int phieuMoiId = 0;

            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var homNay = DateOnly.FromDateTime(
                    DateTime.Today);

                var docGia = await _db.Set<DocGium>()
                    .FirstOrDefaultAsync(
                        x => x.Id == model.DocGiaId);

                if (docGia == null)
                {
                    loi = "Độc giả không tồn tại.";
                }
                else if (docGia.TrangThai != "HoatDong"
                    || docGia.NgayHetHan < homNay)
                {
                    loi = "Thẻ độc giả bị khóa hoặc đã hết hạn.";
                }

                LoaiDocGium? loai = null;

                if (docGia != null && loi == null)
                {
                    loai = await _db.Set<LoaiDocGium>()
                        .FirstOrDefaultAsync(
                            x => x.Id == docGia.LoaiDocGiaId);

                    if (loai == null)
                        loi = "Loại độc giả không hợp lệ.";
                }

                if (loi == null && loai != null)
                {
                    int soSachDangMuon = await (
                        from ct in _db.Set<ChiTietMuon>()
                        join pm in _db.Set<PhieuMuon>()
                            on ct.PhieuMuonId equals pm.Id
                        where pm.DocGiaId == model.DocGiaId
                            && ct.NgayTra == null
                        select ct.Id
                    ).CountAsync();

                    if (soSachDangMuon + ids.Count >
                        loai.SoSachToiDa)
                    {
                        loi = "Vượt quá giới hạn số sách mượn. "
                            + $"Đang mượn: {soSachDangMuon}; "
                            + $"tối đa: {loai.SoSachToiDa}.";
                    }
                }

                var banSaos = new List<BanSaoSach>();

                if (loi == null)
                {
                    banSaos = await _db.Set<BanSaoSach>()
                        .Where(x => ids.Contains(x.Id))
                        .ToListAsync();

                    if (banSaos.Count != ids.Count ||
                        banSaos.Any(x =>
                            x.TrangThai != "SanSang"))
                    {
                        loi = "Có bản sao không sẵn sàng.";
                    }
                }

                if (loi == null)
                {
                    bool daMuon = await _db.Set<ChiTietMuon>()
                        .AnyAsync(x =>
                            ids.Contains(x.BanSaoId) &&
                            x.NgayTra == null);

                    if (daMuon)
                        loi = "Có sách đang được người khác mượn.";
                }

                if (loi == null)
                {
                    var sachIds = banSaos
                        .Select(x => x.SachId)
                        .Distinct()
                        .ToList();

                    int sachHoatDong = await _db.Set<Sach>()
                        .CountAsync(x =>
                            sachIds.Contains(x.Id) &&
                            x.TrangThai == "HoatDong");

                    if (sachHoatDong != sachIds.Count)
                        loi = "Có đầu sách đã ngừng phục vụ.";
                }

                if (loi == null && loai != null)
                {
                    var phieu = new PhieuMuon
                    {
                        // PM + 18 ký tự = 20 ký tự
                        MaPhieu = "PM" +
                            Guid.NewGuid().ToString("N")
                                .Substring(0, 18).ToUpperInvariant(),

                        DocGiaId = model.DocGiaId,
                        NhanVienLapId = nhanVienId,
                        NgayMuon = homNay,
                        TrangThai = "DangMuon",
                        GhiChu = model.GhiChu?.Trim(),
                        NgayTao = DateTime.Now
                    };

                    _db.Set<PhieuMuon>().Add(phieu);
                    await _db.SaveChangesAsync();

                    DateOnly hanTra = homNay.AddDays(
                        loai.SoNgayMuonToiDa);

                    foreach (var bs in banSaos)
                    {
                        var chiTiet = new ChiTietMuon
                        {
                            PhieuMuonId = phieu.Id,
                            BanSaoId = bs.Id,
                            HanTra = hanTra,
                            NgayTra = null,
                            TinhTrangTruoc = bs.TinhTrang
                        };

                        _db.Set<ChiTietMuon>().Add(chiTiet);

                        bs.TrangThai = "DangMuon";
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    phieuMoiId = phieu.Id;
                }
            }
            catch (DbUpdateException)
            {
                loi = "Không thể tạo phiếu. "
                    + "Dữ liệu vừa thay đổi hoặc "
                    + "bản sao đã được mượn.";
            }

            if (loi != null)
            {
                ModelState.AddModelError("", loi);
                await LoadOptions();
                return View(model);
            }

            TempData["Success"] =
                "Tạo phiếu mượn thành công!";

            return RedirectToAction(
                nameof(Details),
                new { id = phieuMoiId });
        }

        // ============================================
        // 5. XEM CHI TIẾT PHIẾU
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var phieu = await (
                from pm in _db.Set<PhieuMuon>().AsNoTracking()
                join dg in _db.Set<DocGium>().AsNoTracking()
                    on pm.DocGiaId equals dg.Id

                where pm.Id == id

                select new PhieuMuonDetailsViewModel
                {
                    Id = pm.Id,
                    MaPhieu = pm.MaPhieu,
                    TenDocGia = dg.HoTen,
                    NgayMuon = pm.NgayMuon,
                    TrangThai = pm.TrangThai
                }
            ).FirstOrDefaultAsync();

            if (phieu == null)
                return NotFound();

            phieu.ChiTiet = await (
                from ct in _db.Set<ChiTietMuon>().AsNoTracking()
                join bs in _db.Set<BanSaoSach>().AsNoTracking()
                    on ct.BanSaoId equals bs.Id
                join s in _db.Set<Sach>().AsNoTracking()
                    on bs.SachId equals s.Id

                where ct.PhieuMuonId == id
                orderby ct.Id

                select new ChiTietMuonItemViewModel
                {
                    Id = ct.Id,
                    MaVach = bs.MaVach,
                    TenSach = s.TieuDe,
                    HanTra = ct.HanTra,
                    NgayTra = ct.NgayTra,
                    TinhTrangTruoc = ct.TinhTrangTruoc,
                    TinhTrangSau = ct.TinhTrangSau
                }
            ).ToListAsync();

            return View(phieu);
        }

        // ============================================
        // 6. XÁC NHẬN TRẢ TỪNG CUỐN SÁCH
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(
            int id, TraSachViewModel model)
        {
            if (model.ChiTietMuonId <= 0 ||
                string.IsNullOrWhiteSpace(model.TinhTrangSau) ||
                !ModelState.IsValid)
            {
                TempData["Error"] =
                    "Thông tin trả sách không hợp lệ.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }

            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                var ct = await _db.Set<ChiTietMuon>()
                    .FirstOrDefaultAsync(x =>
                        x.Id == model.ChiTietMuonId &&
                        x.PhieuMuonId == id);

                if (ct == null)
                    return NotFound();

                if (ct.NgayTra != null)
                {
                    TempData["Error"] =
                        "Cuốn sách này đã được trả.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                var bs = await _db.Set<BanSaoSach>()
                    .FirstOrDefaultAsync(
                        x => x.Id == ct.BanSaoId);

                var phieu = await _db.Set<PhieuMuon>()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (bs == null || phieu == null)
                    return NotFound();

                if (phieu.TrangThai != "DangMuon" ||
                    bs.TrangThai != "DangMuon")
                {
                    TempData["Error"] =
                        "Trạng thái phiếu hoặc bản sao không hợp lệ.";

                    return RedirectToAction(
                        nameof(Details), new { id });
                }

                // Lưu ngày trả
                ct.NgayTra = DateTime.Now;

                // Lưu tình trạng khi nhận lại
                ct.TinhTrangSau =
                    model.TinhTrangSau.Trim();

                ct.GhiChu = model.GhiChu?.Trim();

                if (model.BaoHuHong)
                {
                    bs.TinhTrang = "HuHong";
                    bs.TrangThai = "HuHong";
                }
                else
                {
                    // Giữ tình trạng vật lý trước đó
                    bs.TrangThai = "SanSang";
                }

                await _db.SaveChangesAsync();

                // Kiểm tra còn cuốn nào chưa trả
                bool conSachChuaTra =
                    await _db.Set<ChiTietMuon>()
                        .AnyAsync(x =>
                            x.PhieuMuonId == id &&
                            x.NgayTra == null);

                if (!conSachChuaTra)
                {
                    phieu.TrangThai = "DaTraHet";
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể cập nhật trả sách. "
                    + "Vui lòng thử lại.";

                return RedirectToAction(
                    nameof(Details), new { id });
            }

            TempData["Success"] =
                "Xác nhận trả sách thành công!";

            return RedirectToAction(
                nameof(Details), new { id });
        }
    }
}
