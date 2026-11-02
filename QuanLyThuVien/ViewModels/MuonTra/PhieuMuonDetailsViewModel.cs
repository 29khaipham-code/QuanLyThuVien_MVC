
namespace QuanLyThuVien.ViewModels.MuonTra
{
    public class PhieuMuonDetailsViewModel
    {
        public int Id { get; set; }
        public string MaPhieu { get; set; } = "";
        public string TenDocGia { get; set; } = "";
        public DateOnly NgayMuon { get; set; }
        public string TrangThai { get; set; } = "";

        public List<ChiTietMuonItemViewModel> ChiTiet
        { get; set; } = new();
    }
}
