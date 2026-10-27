using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.HoSo
{
    public class HoSoViewModel
    {
        // ----- Phần được sửa -----
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = "";

        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 số, bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [StringLength(255, ErrorMessage = "Tối đa 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // ----- Phần chỉ hiển thị (không bind từ form) -----
        public string MaDocGia { get; set; } = "";
        public string TenDangNhap { get; set; } = "";
        public string LoaiDocGia { get; set; } = "";
        public int SoSachToiDa { get; set; }
        public int SoNgayMuonToiDa { get; set; }
        public DateOnly NgayDangKy { get; set; }
        public DateOnly NgayHetHan { get; set; }
        public string TrangThai { get; set; } = "";
    }
}