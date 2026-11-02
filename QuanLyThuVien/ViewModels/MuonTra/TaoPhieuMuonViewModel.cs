
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.MuonTra
{
    public class TaoPhieuMuonViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Chọn độc giả")]
        public int DocGiaId { get; set; }

        [Required(ErrorMessage = "Chọn ít nhất một bản sao")]
        public List<int> BanSaoIds { get; set; } = new();

        [StringLength(500)]
        public string? GhiChu { get; set; }
    }
}
