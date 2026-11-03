
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.Admin.Sach
{
    public class SachFormViewModel
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string? Isbn { get; set; }

        [Required(ErrorMessage = "Nhập tên sách")]
        [StringLength(255)]
        public string TieuDe { get; set; } = "";

        [Required(ErrorMessage = "Nhập thể loại")]
        [StringLength(100)]
        public string TenTheLoai { get; set; } = "";

        [Required(ErrorMessage = "Nhập nhà xuất bản")]
        [StringLength(150)]
        public string TenNhaXuatBan { get; set; } = "";

        [Required(ErrorMessage = "Nhập tác giả")]
        public string TenTacGia { get; set; } = "";

        [Range(1, 9999)]
        public int? NamXuatBan { get; set; }

        [Range(1, int.MaxValue)]
        public int? LanTaiBan { get; set; }

        public string? MoTa { get; set; }

        public string? AnhBiaUrl { get; set; }

        public IFormFile? AnhBiaFile { get; set; }

        public string TrangThai { get; set; } = "HoatDong";

        // Tương thích code Controller cũ.
        // Form mới không sử dụng 3 thuộc tính này.
        public int TheLoaiId { get; set; }
        public int NhaXuatBanId { get; set; }
        public List<int> TacGiaIds { get; set; } = new();
    }
}
