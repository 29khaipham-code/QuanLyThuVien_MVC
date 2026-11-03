
namespace QuanLyThuVien.ViewModels.Admin.Phat
{
    public class PhatListItemViewModel
    {
        public int Id { get; set; }
        public int ChiTietMuonId { get; set; }

        public string MaPhieu { get; set; } = "";
        public string MaDocGia { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string TenSach { get; set; } = "";
        public string MaVach { get; set; } = "";

        public string LoaiPhat { get; set; } = "";
        public string? LyDo { get; set; }

        public decimal SoTien { get; set; }
        public bool DaThanhToan { get; set; }

        public DateOnly? NgayThanhToan { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
