using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.DocGia;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public class DatTruocController : DocGiaControllerBase
    {
        
        private const int SoNgayGiuCho = 3;      // số ngày giữ chỗ khi có sách
        private const int SoDongLichSu = 10;

        public DatTruocController(QuanLyThuVienContext db) : base(db)
        {
        }

        // DANH SÁCH ĐẶT TRƯỚC
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            // Giữ chỗ quá hạn thì tự hủy và nhường bản sao cho người kế tiếp
            await XuLyGiuChoQuaHanAsync(dg.Id);

            var tatCa = await Db.DatTruocs.AsNoTracking()
                .Where(d => d.DocGiaId == dg.Id)
                .OrderByDescending(d => d.NgayDat)
                .Select(d => new DatTruocItemViewModel
                {
                    Id = d.Id,
                    SachId = d.SachId,
                    TieuDe = d.Sach.TieuDe,
                    TrangThai = d.TrangThai,
                    NgayDat = d.NgayDat,
                    NgayHetHan = d.NgayHetHan,
                    ViTriHangDoi = d.TrangThai == "DangCho"
                        ? 1 + Db.DatTruocs.Count(x => x.SachId == d.SachId
                                                   && x.TrangThai == "DangCho"
                                                   && x.NgayDat < d.NgayDat)
                        : 0,
                    ViTriKe = d.BanSaoGiu != null ? d.BanSaoGiu.ViTri : null,
                    MaVach = d.BanSaoGiu != null ? d.BanSaoGiu.MaVach : null
                })
                .ToListAsync();

            return View(new DatTruocIndexViewModel
            {
                DangHieuLuc = tatCa.Where(d => d.CoTheHuy).ToList(),
                LichSu = tatCa.Where(d => !d.CoTheHuy).Take(SoDongLichSu).ToList(),
                ToiDa = dg.LoaiDocGia.SoDatTruocToiDa,
                SoNgayGiuCho = SoNgayGiuCho
            });
        }

        // TẠO YÊU CẦU ĐẶT TRƯỚC (từ trang chi tiết sách)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tao(int sachId)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            IActionResult VeChiTiet(string loi)
            {
                TempData["Loi"] = loi;
                return RedirectToAction("ChiTiet", "Sach", new { area = "DocGia", id = sachId });
            }

            // 1. Thẻ phải còn hiệu lực
            if (dg.TrangThai != "HoatDong" || dg.NgayHetHan < DateOnly.FromDateTime(DateTime.Today))
                return VeChiTiet("Thẻ thư viện của bạn đã hết hạn hoặc đang bị khóa, chưa thể đặt trước.");

            // 2. Sách phải tồn tại và có bản giấy nhưng đang không còn bản nào sẵn sàng
            var sach = await Db.Saches.AsNoTracking()
                .Where(s => s.Id == sachId && s.TrangThai == "HoatDong")
                .Select(s => new
                {
                    s.TieuDe,
                    Tong = s.BanSaoSaches.Count(b => b.TrangThai != "DaThanhLy"),
                    SanSang = s.BanSaoSaches.Count(b => b.TrangThai == "SanSang")
                })
                .FirstOrDefaultAsync();

            if (sach == null) return NotFound();

            if (sach.Tong == 0)
                return VeChiTiet("Sách này chưa có bản giấy để đặt trước.");

            if (sach.SanSang > 0)
                return VeChiTiet("Sách đang có sẵn trên kệ, bạn có thể đến thư viện mượn trực tiếp.");

            // 3. Không đặt trước cuốn đang mượn
            var dangMuon = await Db.ChiTietMuons.AnyAsync(c =>
                c.NgayTra == null
                && c.PhieuMuon.DocGiaId == dg.Id
                && c.PhieuMuon.TrangThai != "Huy"
                && c.BanSao.SachId == sachId);

            if (dangMuon)
                return VeChiTiet("Bạn đang mượn cuốn sách này.");

            // 4. Không đặt trùng, không vượt số lượng tối đa
            var dangDat = await Db.DatTruocs
                .Where(d => d.DocGiaId == dg.Id && (d.TrangThai == "DangCho" || d.TrangThai == "DaGiu"))
                .Select(d => d.SachId)
                .ToListAsync();

            if (dangDat.Contains(sachId))
                return VeChiTiet("Bạn đã đặt trước cuốn sách này rồi.");

            var SoDatTruocToiDa = dg.LoaiDocGia.SoDatTruocToiDa;

            if (dangDat.Count >= SoDatTruocToiDa)
                return VeChiTiet($"Bạn chỉ được đặt trước tối đa {SoDatTruocToiDa} cuốn cùng lúc.");

            // 5. Tạo yêu cầu
            Db.DatTruocs.Add(new DatTruoc
            {
                DocGiaId = dg.Id,
                SachId = sachId,
                NgayDat = DateTime.Now,
                TrangThai = "DangCho"
            });

            try
            {
                await Db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Ví dụ bấm hai lần liên tiếp: chỉ mục duy nhất (DocGiaId, SachId) chặn bản ghi thứ hai
                return VeChiTiet("Không thể đặt trước lúc này. Có thể bạn vừa đặt cuốn này, vui lòng kiểm tra lại.");
            }

            TempData["ThanhCong"] = $"Đã đặt trước \"{sach.TieuDe}\". Khi có sách, thư viện sẽ giữ chỗ cho bạn {SoNgayGiuCho} ngày.";
            return RedirectToAction(nameof(Index));
        }

        // HỦY ĐẶT TRƯỚC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            // Chỉ cho hủy yêu cầu của chính mình
            var d = await Db.DatTruocs
                .Include(x => x.BanSaoGiu)
                .FirstOrDefaultAsync(x => x.Id == id && x.DocGiaId == dg.Id);

            if (d == null) return NotFound();

            if (d.TrangThai != "DangCho" && d.TrangThai != "DaGiu")
            {
                TempData["Loi"] = "Yêu cầu này không thể hủy.";
                return RedirectToAction(nameof(Index));
            }

            var dangGiuBanSao = d.TrangThai == "DaGiu";
            d.TrangThai = "Huy";

            // Đang giữ bản sao thì nhường lại cho người kế tiếp (hoặc trả về kệ)
            if (dangGiuBanSao)
                await NhuongBanSaoAsync(d.BanSaoGiu, d.SachId);

            await Db.SaveChangesAsync();

            TempData["ThanhCong"] = "Đã hủy yêu cầu đặt trước.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- Hàm hỗ trợ ----------

        // Bản sao vừa được nhả ra: giao cho người đang chờ sớm nhất, không có thì đưa về SanSang.
        // Chỉ thay đổi dữ liệu trong bộ nhớ, người gọi chịu trách nhiệm SaveChanges.
        private async Task NhuongBanSaoAsync(BanSaoSach? banSao, int sachId)
        {
            if (banSao == null) return;

            var tiep = await Db.DatTruocs
                .Where(x => x.SachId == sachId && x.TrangThai == "DangCho")
                .OrderBy(x => x.NgayDat)
                .FirstOrDefaultAsync();

            if (tiep != null)
            {
                tiep.TrangThai = "DaGiu";
                tiep.BanSaoGiuId = banSao.Id;
                tiep.NgayHetHan = DateTime.Now.AddDays(SoNgayGiuCho);
                banSao.TrangThai = "DaDat";
            }
            else
            {
                banSao.TrangThai = "SanSang";
            }
        }

        // Các yêu cầu DaGiu của độc giả đã quá hạn nhận sách thì chuyển QuaHan và nhường bản sao
        private async Task XuLyGiuChoQuaHanAsync(int docGiaId)
        {
            var now = DateTime.Now;

            var quaHan = await Db.DatTruocs
                .Include(d => d.BanSaoGiu)
                .Where(d => d.DocGiaId == docGiaId && d.TrangThai == "DaGiu" && d.NgayHetHan < now)
                .ToListAsync();

            foreach (var d in quaHan)
            {
                d.TrangThai = "QuaHan";
                await NhuongBanSaoAsync(d.BanSaoGiu, d.SachId);
                await Db.SaveChangesAsync();
            }
        }
    }
}
