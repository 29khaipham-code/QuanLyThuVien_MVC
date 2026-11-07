
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.Admin.MuonTra;

using PhatEntity = QuanLyThuVien.Models.Phat;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri,ThuThu")]
    public class MuonTraController : Controller
    {
        private readonly QuanLyThuVienContext _db;

        private const int PageSize = 10;
        private const decimal PhiQuaHanMoiNgay = 5000m;

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
                total / (double)PageSize);

            if (totalPages > 0)
                page = Math.Min(page, totalPages);

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            return View(data);
        }

        // ============================================
        // 2. DANH SÁCH ĐỘC GIẢ VÀ BẢN SAO
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
                docGias,
                "Id",
                "HienThi");

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
                banSaos,
                "Id",
                "HienThi");
        }

        // ============================================
        // 3. FORM TẠO PHIẾU
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

            model.BanSaoIds = ids;

            if (model.DocGiaId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.DocGiaId),
                    "Vui lòng chọn độc giả.");
            }

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

            // Lấy tài khoản nhân viên đang đăng nhập
            string? userIdText = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                userIdText,
                out int nhanVienId))
            {
                return Forbid();
            }

            bool nhanVienHopLe =
                await _db.Set<TaiKhoan>()
                    .AnyAsync(x =>
                        x.Id == nhanVienId &&
                        x.HoatDong);

            if (!nhanVienHopLe)
                return Forbid();

            string? loi = null;
            int phieuMoiId = 0;

            try
            {
                await using var transaction =
                    await _db.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable);

                DateOnly homNay =
                    DateOnly.FromDateTime(DateTime.Today);

                // Kiểm tra độc giả
                var docGia = await _db.Set<DocGium>()
                    .FirstOrDefaultAsync(
                        x => x.Id == model.DocGiaId);

                if (docGia == null)
                {
                    loi = "Độc giả không tồn tại.";
                }
                else if (docGia.TrangThai != "HoatDong" ||
                         docGia.NgayHetHan < homNay)
                {
                    loi = "Thẻ độc giả đã khóa hoặc hết hạn.";
                }

                // Loại độc giả quyết định giới hạn mượn
                LoaiDocGium? loaiDocGia = null;

                if (loi == null && docGia != null)
                {
                    loaiDocGia =
                        await _db.Set<LoaiDocGium>()
                            .FirstOrDefaultAsync(
                                x => x.Id ==
                                     docGia.LoaiDocGiaId);

                    if (loaiDocGia == null)
                        loi = "Loại độc giả không hợp lệ.";
                }

                // Kiểm tra số lượng sách đang mượn
                if (loi == null && loaiDocGia != null)
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
                        loaiDocGia.SoSachToiDa)
                    {
                        loi =
                            $"Độc giả đang mượn {soSachDangMuon} cuốn. " +
                            $"Giới hạn tối đa {loaiDocGia.SoSachToiDa} cuốn.";
                    }
                }

                // Kiểm tra bản sao sách
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
                        loi = "Có bản sao không sẵn sàng để mượn.";
                    }
                }

                // Chặn bản sao đang thuộc lượt mượn khác
                if (loi == null)
                {
                    bool coSachDangMuon =
                        await _db.Set<ChiTietMuon>()
                            .AnyAsync(x =>
                                ids.Contains(x.BanSaoId) &&
                                x.NgayTra == null);

                    if (coSachDangMuon)
                    {
                        loi =
                            "Có bản sao đang được người khác mượn.";
                    }
                }

                // Kiểm tra đầu sách còn hoạt động
                if (loi == null)
                {
                    var sachIds = banSaos
                        .Select(x => x.SachId)
                        .Distinct()
                        .ToList();

                    int sachHoatDong =
                        await _db.Set<Sach>()
                            .CountAsync(x =>
                                sachIds.Contains(x.Id) &&
                                x.TrangThai == "HoatDong");

                    if (sachHoatDong != sachIds.Count)
                    {
                        loi =
                            "Có đầu sách đã ngừng phục vụ.";
                    }
                }

                // Lưu phiếu và chi tiết mượn
                if (loi == null && loaiDocGia != null)
                {
                    var phieu = new PhieuMuon
                    {
                        MaPhieu = "PM" +
                            Guid.NewGuid()
                                .ToString("N")
                                .Substring(0, 18)
                                .ToUpperInvariant(),

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
                        loaiDocGia.SoNgayMuonToiDa);

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
                loi =
                    "Không thể tạo phiếu mượn. " +
                    "Dữ liệu đã thay đổi hoặc sách đã được mượn.";
            }
            catch (Exception)
            {
                loi =
                    "Có lỗi khi tạo phiếu mượn. " +
                    "Vui lòng kiểm tra và thử lại.";
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
        // 5. XEM CHI TIẾT PHIẾU MƯỢN
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
        // 6. TRẢ SÁCH + TỰ ĐỘNG PHẠT QUÁ HẠN
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

            string? loi = null;
            int soNgayTre = 0;
            decimal tienPhat = 0m;
            bool phatMoi = false;

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
                {
                    loi = "Không tìm thấy lượt mượn.";
                }
                else if (ct.NgayTra != null)
                {
                    loi = "Cuốn sách này đã được trả trước đó.";
                }

                PhieuMuon? phieu = null;
                BanSaoSach? banSao = null;

                if (loi == null && ct != null)
                {
                    phieu = await _db.Set<PhieuMuon>()
                        .FirstOrDefaultAsync(x => x.Id == id);

                    banSao = await _db.Set<BanSaoSach>()
                        .FirstOrDefaultAsync(
                            x => x.Id == ct.BanSaoId);

                    if (phieu == null || banSao == null)
                    {
                        loi =
                            "Không tìm thấy phiếu hoặc bản sao.";
                    }
                    else if (phieu.TrangThai != "DangMuon")
                    {
                        loi =
                            "Phiếu mượn không ở trạng thái đang mượn.";
                    }
                    else if (banSao.TrangThai != "DangMuon")
                    {
                        loi =
                            "Bản sao không ở trạng thái đang mượn.";
                    }
                }

                if (loi == null &&
                    ct != null &&
                    phieu != null &&
                    banSao != null)
                {
                    DateTime ngayTraThucTe = DateTime.Now;

                    DateOnly ngayTra =
                        DateOnly.FromDateTime(ngayTraThucTe);

                    // Tính số ngày trả trễ
                    soNgayTre = Math.Max(
                        0,
                        ngayTra.DayNumber -
                        ct.HanTra.DayNumber);

                    tienPhat =
                        soNgayTre * PhiQuaHanMoiNgay;

                    // Cập nhật thông tin trả
                    ct.NgayTra = ngayTraThucTe;
                    ct.TinhTrangSau =
                        model.TinhTrangSau.Trim();
                    ct.GhiChu =
                        model.GhiChu?.Trim();

                    // Cập nhật tình trạng bản sao
                    if (model.BaoHuHong)
                    {
                        banSao.TinhTrang = "HuHong";
                        banSao.TrangThai = "HuHong";
                    }
                    else if (banSao.TinhTrang == "HuHong")
                    {
                        // Không tự mở phục vụ sách hỏng
                        banSao.TrangThai = "HuHong";
                    }
                    else
                    {
                        banSao.TrangThai = "SanSang";
                    }

                    await _db.SaveChangesAsync();

                    // Tự tạo phạt trả muộn
                    if (soNgayTre > 0)
                    {
                        bool daCoPhat =
                            await _db.Set<PhatEntity>()
                                .AnyAsync(x =>
                                    x.ChiTietMuonId == ct.Id &&
                                    x.LoaiPhat == "TraMuon");

                        if (!daCoPhat)
                        {
                            var phat = new PhatEntity
                            {
                                ChiTietMuonId = ct.Id,
                                LoaiPhat = "TraMuon",
                                LyDo =
                                    $"Trả muộn {soNgayTre} ngày, " +
                                    $"phí {PhiQuaHanMoiNgay:N0}đ/ngày.",
                                SoTien = tienPhat,
                                DaThanhToan = false,
                                NgayThanhToan = null,
                                NgayTao = DateTime.Now
                            };

                            _db.Set<PhatEntity>().Add(phat);
                            phatMoi = true;
                        }
                    }

                    // Kiểm tra toàn phiếu đã trả hết chưa
                    bool conChuaTra =
                        await _db.Set<ChiTietMuon>()
                            .AnyAsync(x =>
                                x.PhieuMuonId == id &&
                                x.NgayTra == null);

                    if (!conChuaTra)
                    {
                        phieu.TrangThai = "DaTraHet";
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch (DbUpdateException)
            {
                loi =
                    "Không thể xác nhận trả sách. " +
                    "Dữ liệu đã thay đổi hoặc trùng khoản phạt.";
            }
            catch (Exception)
            {
                loi =
                    "Có lỗi khi xử lý trả sách. " +
                    "Vui lòng thử lại.";
            }

            if (loi != null)
            {
                TempData["Error"] = loi;

                return RedirectToAction(
                    nameof(Details), new { id });
            }

            if (phatMoi)
            {
                TempData["Success"] =
                    "Trả sách thành công! " +
                    $"Quá hạn {soNgayTre} ngày, " +
                    $"đã tạo khoản phạt {tienPhat:N0}đ " +
                    "(chưa thanh toán).";
            }
            else if (soNgayTre > 0)
            {
                TempData["Success"] =
                    "Trả sách thành công. " +
                    "Khoản phạt quá hạn đã tồn tại.";
            }
            else
            {
                TempData["Success"] =
                    "Trả sách thành công, không quá hạn.";
            }

            return RedirectToAction(
                nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> GoiY(string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Array.Empty<object>());

            term = term.Trim();

            var data = await (
                from pm in _db.Set<PhieuMuon>().AsNoTracking()
                join dg in _db.Set<DocGium>().AsNoTracking()
                    on pm.DocGiaId equals dg.Id
                where pm.MaPhieu.Contains(term)
                   || dg.HoTen.Contains(term)
                orderby pm.MaPhieu.StartsWith(term) descending,
                        pm.Id descending
                select new
                {
                    value = pm.MaPhieu,
                    label = dg.HoTen,
                    detail = pm.MaPhieu + " | " + dg.HoTen
                }
            ).Take(10).ToListAsync();

            return Json(data);
        }

    }
}
