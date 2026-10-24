
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels
{
    public class DangNhapViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = "";

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool GhiNho { get; set; }
    }
}
