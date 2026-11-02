
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.BanSaoSach
{
    public class BanSaoSachFormViewModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn sách")]
        [Display(Name = "Đầu sách")]
        public int SachId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã vạch")]
        [StringLength(50)]
        [Display(Name = "Mã vạch")]
        public string MaVach { get; set; } = "";

        [Required]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "SanSang";

        [Required]
        [Display(Name = "Tình trạng")]
        public string TinhTrang { get; set; } = "Tot";

        [DataType(DataType.Date)]
        [Display(Name = "Ngày nhập")]
        public DateOnly? NgayNhap { get; set; }

        [StringLength(100)]
        [Display(Name = "Vị trí")]
        public string? ViTri { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999",
            ErrorMessage = "Giá trị phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá trị (VNĐ)")]
        public decimal? GiaTri { get; set; }
    }

    public class BanSaoSachListItemViewModel
    {
        public int Id { get; set; }
        public int SachId { get; set; }
        public string TenSach { get; set; } = "";
        public string MaVach { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public string TinhTrang { get; set; } = "";
        public DateOnly? NgayNhap { get; set; }
        public string? ViTri { get; set; }
        public decimal? GiaTri { get; set; }
    }
}
