
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.TaiKhoan
{
    public class TaoTaiKhoanViewModel
    {
        [Required(ErrorMessage = "Nhập tên đăng nhập")]
        [StringLength(50, MinimumLength = 3)]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Nhập họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Nhập email")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = "";

        [StringLength(20)]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Nhập mật khẩu")]
        [MinLength(8, ErrorMessage = "Tối thiểu 8 ký tự")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";

        [Required(ErrorMessage = "Chọn vai trò")]
        public int? VaiTroId { get; set; }
    }
}
