using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.DocGia
{
    // Trang xem hồ sơ
    public class HoSoViewModel
    {
        public string TenDangNhap { get; set; } = "";
        public string MaDocGia { get; set; } = "";
        public string HoTen { get; set; } = "";
        public DateOnly? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string? AnhDaiDienUrl { get; set; }

        // Thẻ thư viện
        public string LoaiDocGia { get; set; } = "";
        public int SoSachToiDa { get; set; }
        public int SoNgayMuonToiDa { get; set; }
        public DateOnly NgayDangKy { get; set; }
        public DateOnly NgayHetHan { get; set; }
        public string TrangThai { get; set; } = "";

        // Thống kê nhanh
        public int DangMuon { get; set; }
        public int TongLuotMuon { get; set; }
        public decimal PhatChuaNop { get; set; }

        public bool TheConHieuLuc =>
            TrangThai == "HoatDong" && NgayHetHan >= DateOnly.FromDateTime(DateTime.Today);
    }

    // Form sửa thông tin cá nhân
    public class SuaHoSoViewModel
    {
        // Chỉ hiển thị, không cho sửa
        public string MaDocGia { get; set; } = "";
        public string TenDangNhap { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = "";

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateOnly? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        [RegularExpression("^(Nam|Nữ|Khác)?$", ErrorMessage = "Giới tính không hợp lệ")]
        public string? GioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [RegularExpression(@"^[\x20-\x7E]+$", ErrorMessage = "Email chỉ gồm ký tự không dấu")]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [RegularExpression(@"^(0\d{9}|\+84\d{9})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
    }

    // Form đổi mật khẩu
    public class DoiMatKhauViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string MatKhauHienTai { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 đến 100 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Mật khẩu phải có cả chữ và số")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        public string XacNhanMatKhau { get; set; } = "";
    }
}
