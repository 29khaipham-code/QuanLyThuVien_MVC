
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.Admin.Phat
{
    public class PhatFormViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn lượt mượn")]
        public int ChiTietMuonId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại phạt")]
        public string LoaiPhat { get; set; } = "TraMuon";

        [StringLength(500)]
        public string? LyDo { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999",
            ErrorMessage = "Số tiền không hợp lệ")]
        public decimal SoTien { get; set; }
    }
}
