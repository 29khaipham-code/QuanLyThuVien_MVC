using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.Services;
using QuanLyThuVien.ViewModels.DocGia;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public class ThongBaoController : DocGiaControllerBase
    {
        private const int KichThuocTrang = 10;

        private readonly ThongBaoService _thongBao;

        public ThongBaoController(QuanLyThuVienContext db, ThongBaoService thongBao) : base(db)
        {
            _thongBao = thongBao;
        }

        // DANH SÁCH THÔNG BÁO (lọc + phân trang, AJAX trả về partial)

        [HttpGet]
        public async Task<IActionResult> Index(string? loai, string? trangThai = "tatca", int trang = 1)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            await _thongBao.QuetNeuCanAsync(dg.Id);

            if (!LoaiThongBao.HopLe(loai)) loai = null;
            if (trangThai != "chuadoc" && trangThai != "dadoc") trangThai = "tatca";

            var goc = Db.ThongBaos.AsNoTracking()
                .Where(t => t.DocGiaId == dg.Id && !t.DaXoa);

            var query = goc;

            if (loai != null)
                query = query.Where(t => t.Loai == loai);

            if (trangThai == "chuadoc")
                query = query.Where(t => !t.DaDoc);
            else if (trangThai == "dadoc")
                query = query.Where(t => t.DaDoc);

            var tongSo = await query.CountAsync();
            var tongTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)KichThuocTrang));
            trang = Math.Clamp(trang, 1, tongTrang);

            var items = await query
                .OrderByDescending(t => t.NgayTao).ThenByDescending(t => t.Id)
                .Skip((trang - 1) * KichThuocTrang)
                .Take(KichThuocTrang)
                .Select(t => new ThongBaoItemViewModel
                {
                    Id = t.Id,
                    Loai = t.Loai,
                    TieuDe = t.TieuDe,
                    NoiDung = t.NoiDung,
                    Url = t.Url,
                    DaDoc = t.DaDoc,
                    NgayTao = t.NgayTao
                })
                .ToListAsync();

            var vm = new ThongBaoIndexViewModel
            {
                Loai = loai,
                TrangThai = trangThai,
                Trang = trang,
                KichThuocTrang = KichThuocTrang,
                TongSo = tongSo,
                ChuaDoc = await goc.CountAsync(t => !t.DaDoc),
                Items = items
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DanhSach", vm);

            return View(vm);
        }

        // =====================================================
        // MỞ MỘT THÔNG BÁO: đánh dấu đã đọc rồi chuyển tới trang liên quan
        // (đổi trạng thái bằng GET vì thao tác này an toàn và lặp lại nhiều lần cũng không sao)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Mo(int id)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            var tb = await Db.ThongBaos
                .FirstOrDefaultAsync(t => t.Id == id && t.DocGiaId == dg.Id && !t.DaXoa);

            if (tb == null) return NotFound();

            if (!tb.DaDoc)
            {
                tb.DaDoc = true;
                tb.NgayDoc = DateTime.Now;
                await Db.SaveChangesAsync();
            }

            // Chỉ chuyển tới đường dẫn nội bộ
            if (!string.IsNullOrEmpty(tb.Url) && Url.IsLocalUrl(tb.Url))
                return LocalRedirect(tb.Url);

            return RedirectToAction(nameof(Index));
        }


        // CÁC THAO TÁC (gọi bằng AJAX từ trang danh sách)
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DanhDauDaDoc(int id)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return Forbid();

            DateTime? bayGio = DateTime.Now;

            await Db.ThongBaos
                .Where(t => t.Id == id && t.DocGiaId == dg.Id && !t.DaXoa && !t.DaDoc)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.DaDoc, true)
                    .SetProperty(t => t.NgayDoc, bayGio));

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DocTatCa()
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return Forbid();

            DateTime? bayGio = DateTime.Now;

            await Db.ThongBaos
                .Where(t => t.DocGiaId == dg.Id && !t.DaXoa && !t.DaDoc)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.DaDoc, true)
                    .SetProperty(t => t.NgayDoc, bayGio));

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return Forbid();

            // Xóa mềm: giữ lại dòng để khóa chống trùng còn hiệu lực
            await Db.ThongBaos
                .Where(t => t.Id == id && t.DocGiaId == dg.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.DaXoa, true));

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaDaDoc()
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return Forbid();

            await Db.ThongBaos
                .Where(t => t.DocGiaId == dg.Id && !t.DaXoa && t.DaDoc)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.DaXoa, true));

            return Ok();
        }


        // LÀM MỚI QUẢ CHUÔNG (trình duyệt gọi định kỳ)

        [HttpGet]
        public IActionResult Chuong() => ViewComponent("ThongBaoChuong");
    }
}
