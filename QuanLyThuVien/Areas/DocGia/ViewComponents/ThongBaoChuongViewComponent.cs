using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.Services;
using QuanLyThuVien.ViewModels.DocGia;
using System.Security.Claims;

namespace QuanLyThuVien.Areas.DocGia.ViewComponents
{
    // Quả chuông thông báo trên navbar: số chưa đọc và 5 thông báo mới nhất
    public class ThongBaoChuongViewComponent : ViewComponent
    {
        private const int SoThongBaoHienThi = 5;

        private readonly QuanLyThuVienContext _db;
        private readonly ThongBaoService _thongBao;

        public ThongBaoChuongViewComponent(QuanLyThuVienContext db, ThongBaoService thongBao)
        {
            _db = db;
            _thongBao = thongBao;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!int.TryParse(UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out var taiKhoanId))
                return Content(string.Empty);

            var docGiaId = await _db.DocGia.AsNoTracking()
                .Where(d => d.TaiKhoanId == taiKhoanId)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();

            if (docGiaId == null)
                return Content(string.Empty);

            // Cập nhật thông báo mới (có giới hạn tần suất nên gọi ở mỗi trang cũng không nặng)
            await _thongBao.QuetNeuCanAsync(docGiaId.Value);

            var goc = _db.ThongBaos.AsNoTracking()
                .Where(t => t.DocGiaId == docGiaId.Value && !t.DaXoa);

            var vm = new ChuongViewModel
            {
                ChuaDoc = await goc.CountAsync(t => !t.DaDoc),
                Items = await goc
                    .OrderByDescending(t => t.NgayTao).ThenByDescending(t => t.Id)
                    .Take(SoThongBaoHienThi)
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
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
