namespace QuanLyThuVien.ViewModels.DocGia
{
    // Một yêu cầu đặt trước
    public class DatTruocItemViewModel
    {
        public int Id { get; set; }
        public int SachId { get; set; }
        public string TieuDe { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public DateTime NgayDat { get; set; }
        public DateTime? NgayHetHan { get; set; }   // hạn nhận sách khi đã giữ chỗ
        public int ViTriHangDoi { get; set; }       // chỉ có nghĩa khi DangCho
        public string? ViTriKe { get; set; }        // vị trí bản sao đang giữ
        public string? MaVach { get; set; }

        public bool CoTheHuy => TrangThai == "DangCho" || TrangThai == "DaGiu";

        public string TrangThaiText => TrangThai switch
        {
            "DangCho" => "Đang chờ",
            "DaGiu" => "Đã giữ chỗ",
            "DaNhan" => "Đã nhận",
            "QuaHan" => "Quá hạn giữ chỗ",
            "Huy" => "Đã hủy",
            _ => TrangThai
        };

        public string LopBadge => TrangThai switch
        {
            "DangCho" => "bg-warning text-dark",
            "DaGiu" => "bg-success",
            "DaNhan" => "bg-info text-dark",
            _ => "bg-secondary"
        };
    }

    public class DatTruocIndexViewModel
    {
        public List<DatTruocItemViewModel> DangHieuLuc { get; set; } = new();
        public List<DatTruocItemViewModel> LichSu { get; set; } = new();
        public int ToiDa { get; set; }
        public int SoNgayGiuCho { get; set; }
    }
}
