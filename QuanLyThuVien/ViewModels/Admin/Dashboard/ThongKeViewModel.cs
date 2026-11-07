namespace QuanLyThuVien.ViewModels.Admin.Dashboard
{
    public class ThongKeViewModel
    {
        public int TongDauSach { get; set; }

        public int TongDocGia { get; set; }

        public int SachDangMuon { get; set; }

        public int SachQuaHan { get; set; }

        public List<ThongKeTheoThangViewModel> MuonTheoThang { get; set; } = new();

        public List<ThongKeTheLoaiViewModel> SachTheoTheLoai { get; set; } = new();

        public List<SachMuonNhieuViewModel> SachMuonNhieu { get; set; } = new();
    }

    public class ThongKeTheoThangViewModel
    {
        public string Thang { get; set; } = string.Empty;

        public int SoLuotMuon { get; set; }
    }

    public class ThongKeTheLoaiViewModel
    {
        public string TenTheLoai { get; set; } = string.Empty;

        public int SoDauSach { get; set; }
    }

    public class SachMuonNhieuViewModel
    {
        public string TenSach { get; set; } = string.Empty;

        public int SoLuotMuon { get; set; }
    }
}
