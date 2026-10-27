
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.TheLoai
{
    public class TheLoaiFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thể loại")]
        [StringLength(100)]
        [Display(Name = "Tên thể loại")]
        public string TenTheLoai { get; set; } = "";

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
    }
}
