namespace QuanLyThuVien.ViewModels.Admin.MuonTra
{
    public class PhieuMuonListItemViewModel
    {
        public int Id { get; set; }
        public string MaPhieu { get; set; } = "";
        public string TenDocGia { get; set; } = "";
        public DateOnly NgayMuon { get; set; }
        public string TrangThai { get; set; } = "";
        public int SoCuon { get; set; }
        public int ChuaTra { get; set; }
    }
}
