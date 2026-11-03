namespace QuanLyThuVien.ViewModels.Admin.MuonTra
{
    public class ChiTietMuonItemViewModel
    {
        public int Id { get; set; }
        public string MaVach { get; set; } = "";
        public string TenSach { get; set; } = "";
        public DateOnly HanTra { get; set; }
        public DateTime? NgayTra { get; set; }
        public string TinhTrangTruoc { get; set; } = "";
        public string? TinhTrangSau { get; set; }
    }
}
