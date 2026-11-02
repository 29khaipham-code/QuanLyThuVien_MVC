using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels.DocGia;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    [Area("DocGia")]
    [Authorize(Roles = "DocGia")]
    public class MuonTraController : DocGiaControllerBase
    {
        private const int KichThuocTrang = 10;

        // Chiếu ChiTietMuon sang MuonItemViewModel (dùng cho cả "Đang mượn" và "Lịch sử")
        private static readonly Expression<Func<ChiTietMuon, MuonItemViewModel>> ChieuMuon = c => new MuonItemViewModel
        {
            ChiTietId = c.Id,
            SachId = c.BanSao.SachId,
            TieuDe = c.BanSao.Sach.TieuDe,
            MaVach = c.BanSao.MaVach,
            ViTri = c.BanSao.ViTri,
            MaPhieu = c.PhieuMuon.MaPhieu,
            NgayMuon = c.PhieuMuon.NgayMuon,
            HanTra = c.HanTra,
            NgayTra = c.NgayTra,
            TongPhat = c.Phats.Sum(p => (decimal?)p.SoTien) ?? 0,
            PhatChuaNop = c.Phats.Where(p => !p.DaThanhToan).Sum(p => (decimal?)p.SoTien) ?? 0
        };

        public MuonTraController(QuanLyThuVienContext db) : base(db)
        {
        }

        // TAB 1: SÁCH ĐANG MƯỢN
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            var items = await Db.ChiTietMuons.AsNoTracking()
                .Where(c => c.PhieuMuon.DocGiaId == dg.Id
                         && c.PhieuMuon.TrangThai != "Huy"
                         && c.NgayTra == null)
                .OrderBy(c => c.HanTra)
                .Select(ChieuMuon)
                .ToListAsync();

            return View(new DangMuonViewModel
            {
                Items = items,
                ToiDa = dg.LoaiDocGia.SoSachToiDa,
                SoNgayMuonToiDa = dg.LoaiDocGia.SoNgayMuonToiDa
            });
        }

        // TAB 2: LỊCH SỬ MƯỢN (lọc + phân trang, AJAX trả về partial)
        [HttpGet]
        public async Task<IActionResult> LichSu(
            string? tuKhoa, string? trangThai = "tatca",
            DateOnly? tuNgay = null, DateOnly? denNgay = null, int trang = 1)
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            tuKhoa = tuKhoa?.Trim();
            trangThai ??= "tatca";

            var query = Db.ChiTietMuons.AsNoTracking()
                .Where(c => c.PhieuMuon.DocGiaId == dg.Id && c.PhieuMuon.TrangThai != "Huy");

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(c =>
                    c.BanSao.Sach.TieuDe.Contains(tuKhoa) ||
                    c.PhieuMuon.MaPhieu.Contains(tuKhoa));
            }

            if (tuNgay.HasValue)
                query = query.Where(c => c.PhieuMuon.NgayMuon >= tuNgay.Value);

            if (denNgay.HasValue)
                query = query.Where(c => c.PhieuMuon.NgayMuon <= denNgay.Value);

            query = trangThai switch
            {
                "dangmuon" => query.Where(c => c.NgayTra == null),
                "datra" => query.Where(c => c.NgayTra != null),
                "tramuon" => query.Where(c => c.NgayTra != null
                                            && DateOnly.FromDateTime(c.NgayTra.Value) > c.HanTra),
                _ => query
            };

            query = query
                .OrderByDescending(c => c.PhieuMuon.NgayMuon)
                .ThenByDescending(c => c.Id);

            var tongSo = await query.CountAsync();
            var tongTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)KichThuocTrang));
            trang = Math.Clamp(trang, 1, tongTrang);

            var items = await query
                .Skip((trang - 1) * KichThuocTrang)
                .Take(KichThuocTrang)
                .Select(ChieuMuon)
                .ToListAsync();

            var vm = new LichSuMuonViewModel
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                Trang = trang,
                KichThuocTrang = KichThuocTrang,
                TongSo = tongSo,
                Items = items
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_LichSuDanhSach", vm);

            return View(vm);
        }

        // TAB 3: TIỀN PHẠT
        [HttpGet]
        public async Task<IActionResult> Phat(string? loc = "tatca")
        {
            var dg = await LayDocGiaAsync();
            if (dg == null) return ChuaCoHoSo();

            loc ??= "tatca";

            var query = Db.Phats.AsNoTracking()
                .Where(p => p.ChiTietMuon.PhieuMuon.DocGiaId == dg.Id);

            var tongChuaNop = await query.Where(p => !p.DaThanhToan)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;
            var tongDaNop = await query.Where(p => p.DaThanhToan)
                .SumAsync(p => (decimal?)p.SoTien) ?? 0;

            if (loc == "chuanop")
                query = query.Where(p => !p.DaThanhToan);
            else if (loc == "danop")
                query = query.Where(p => p.DaThanhToan);

            var items = await query
                .OrderByDescending(p => p.NgayTao)
                .Take(200)
                .Select(p => new PhatItemViewModel
                {
                    Id = p.Id,
                    LoaiPhat = p.LoaiPhat,
                    LyDo = p.LyDo,
                    SoTien = p.SoTien,
                    DaThanhToan = p.DaThanhToan,
                    NgayThanhToan = p.NgayThanhToan,
                    NgayTao = p.NgayTao,
                    TieuDe = p.ChiTietMuon.BanSao.Sach.TieuDe,
                    MaPhieu = p.ChiTietMuon.PhieuMuon.MaPhieu
                })
                .ToListAsync();

            return View(new PhatViewModel
            {
                Loc = loc,
                TongChuaNop = tongChuaNop,
                TongDaNop = tongDaNop,
                Items = items
            });
        }
    }
}
