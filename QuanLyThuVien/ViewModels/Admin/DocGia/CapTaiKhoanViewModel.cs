
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.Admin.DocGia
{
    public class CapTaiKhoanViewModel
    {
        public int DocGiaId { get; set; }

        public string MaDocGia { get; set; } = "";
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Nhập tên đăng nhập")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Nhập email")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Nhập mật khẩu")]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";
    }
}
