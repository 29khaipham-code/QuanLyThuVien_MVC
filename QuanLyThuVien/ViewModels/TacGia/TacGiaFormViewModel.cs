
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.TacGia
{
    public class TacGiaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tác giả")]
        [StringLength(150, ErrorMessage = "Tên tối đa 150 ký tự")]
        [Display(Name = "Tên tác giả")]
        public string TenTacGia { get; set; } = "";

        [Display(Name = "Tiểu sử")]
        public string? TieuSu { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateOnly? NgaySinh { get; set; }

        [StringLength(100, ErrorMessage = "Tối đa 100 ký tự")]
        [Display(Name = "Quốc tịch")]
        public string? QuocTich { get; set; }
    }
}
