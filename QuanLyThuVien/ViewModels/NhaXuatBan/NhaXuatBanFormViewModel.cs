
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.NhaXuatBan
{
    public class NhaXuatBanFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhà xuất bản")]
        [StringLength(150, ErrorMessage = "Tên tối đa 150 ký tự")]
        [Display(Name = "Tên nhà xuất bản")]
        public string TenNhaXuatBan { get; set; } = "";

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "Website tối đa 255 ký tự")]
        [Url(ErrorMessage = "Website phải là URL hợp lệ, ví dụ https://example.com")]
        [Display(Name = "Website")]
        public string? Website { get; set; }
    }
}
