
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.Sach
{
    public class SachFormViewModel
    {
        public int Id { get; set; }

        [StringLength(20)]
        [Display(Name = "ISBN")]
        public string? Isbn { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sách")]
        [StringLength(255)]
        [Display(Name = "Tên sách")]
        public string TieuDe { get; set; } = "";

        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn thể loại")]
        public int TheLoaiId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn nhà xuất bản")]
        public int NhaXuatBanId { get; set; }

        [Display(Name = "Tác giả")]
        public List<int> TacGiaIds { get; set; } = new();

        [Range(1, 9999)]
        public int? NamXuatBan { get; set; }

        [Range(1, int.MaxValue)]
        public int? LanTaiBan { get; set; }

        public string? MoTa { get; set; }

        [StringLength(255)]
        public string? AnhBiaUrl { get; set; }

        [Required]
        public string TrangThai { get; set; } = "HoatDong";
    }
}
