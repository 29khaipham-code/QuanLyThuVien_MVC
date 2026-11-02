
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.MuonTra
{
    public class TraSachViewModel
    {
        public int ChiTietMuonId { get; set; }

        [Required]
        [StringLength(255)]
        public string TinhTrangSau { get; set; } = "";

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public bool BaoHuHong { get; set; }
    }
}
