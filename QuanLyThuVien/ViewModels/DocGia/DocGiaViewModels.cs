
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.DocGia
{
    public class DocGiaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã độc giả")]
        [StringLength(20)]
        [Display(Name = "Mã độc giả")]
        public string MaDocGia { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = "";

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateOnly? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày đăng ký")]
        public DateOnly NgayDangKy { get; set; }
            = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày hết hạn")]
        public DateOnly NgayHetHan { get; set; }
            = DateOnly.FromDateTime(DateTime.Today.AddYears(1));

        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn loại độc giả")]
        [Display(Name = "Loại độc giả")]
        public int LoaiDocGiaId { get; set; }

        [Display(Name = "Tài khoản website")]
        public int? TaiKhoanId { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "HoatDong";
    }

    public class DocGiaListItemViewModel
    {
        public int Id { get; set; }
        public string MaDocGia { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string TenLoai { get; set; } = "";
        public DateOnly NgayHetHan { get; set; }
        public string TrangThai { get; set; } = "";
        public int? TaiKhoanId { get; set; }
    }
}
