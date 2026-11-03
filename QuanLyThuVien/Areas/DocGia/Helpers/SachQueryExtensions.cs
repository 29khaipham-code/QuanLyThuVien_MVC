using QuanLyThuVien.Models;
using QuanLyThuVien.ViewModels;
using QuanLyThuVien.ViewModels.DocGia;

namespace QuanLyThuVien.Areas.DocGia.Helpers
{
    public static class SachQueryExtensions
    {
        // Chiếu Sach thành SachItemViewModel (dùng chung cho Trang chủ và Tra cứu sách)
        public static IQueryable<SachItemViewModel> ToItemViewModel(this IQueryable<Sach> query)
        {
            return query.Select(s => new SachItemViewModel
            {
                Id = s.Id,
                TieuDe = s.TieuDe,
                AnhBiaUrl = s.AnhBiaUrl,
                TheLoai = s.TheLoai.TenTheLoai,
                NhaXuatBan = s.NhaXuatBan.TenNhaXuatBan,
                NamXuatBan = s.NamXuatBan,
                TacGias = s.TacGia.Select(t => t.TenTacGia).ToList(),
                TongBanSao = s.BanSaoSaches.Count(b => b.TrangThai != "DaThanhLy"),
                SanSang = s.BanSaoSaches.Count(b => b.TrangThai == "SanSang"),
                CoSachDienTu = s.TepSachDienTus.Any(t => t.TrangThai == "HoatDong")
            });
        }
    }
}
