namespace QuanLyThuVien.ViewModels.DocGia
{
    // Một dòng chi tiết mượn (một cuốn sách trong một phiếu)
    public class MuonItemViewModel
    {
        public int ChiTietId { get; set; }
        public int SachId { get; set; }
        public string TieuDe { get; set; } = "";
        public string MaVach { get; set; } = "";
        public string? ViTri { get; set; }
        public string MaPhieu { get; set; } = "";
        public DateOnly NgayMuon { get; set; }
        public DateOnly HanTra { get; set; }
        public DateTime? NgayTra { get; set; }
        public decimal TongPhat { get; set; }
        public decimal PhatChuaNop { get; set; }

        public bool DangMuon => NgayTra == null;

        // Số ngày còn lại tới hạn trả (âm = đã quá hạn)
        public int SoNgayConLai => HanTra.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;

        public bool QuaHan => DangMuon && SoNgayConLai < 0;

        public bool TraMuon => NgayTra.HasValue && DateOnly.FromDateTime(NgayTra.Value) > HanTra;

        public string TrangThaiText =>
            DangMuon
                ? (QuaHan ? $"Quá hạn {-SoNgayConLai} ngày"
                   : SoNgayConLai == 0 ? "Đến hạn hôm nay"
                   : $"Còn {SoNgayConLai} ngày")
                : (TraMuon ? "Đã trả (muộn)" : "Đã trả đúng hạn");

        public string LopBadge =>
            DangMuon
                ? (QuaHan ? "bg-danger" : SoNgayConLai <= 3 ? "bg-warning text-dark" : "bg-success")
                : (TraMuon ? "bg-secondary" : "bg-info text-dark");
    }

    // Tab "Đang mượn"
    public class DangMuonViewModel
    {
        public List<MuonItemViewModel> Items { get; set; } = new();
        public int ToiDa { get; set; }
        public int SoNgayMuonToiDa { get; set; }

        public int SoQuaHan => Items.Count(i => i.QuaHan);
    }

    // Tab "Lịch sử mượn": bộ lọc + phân trang
    public class LichSuMuonViewModel
    {
        public string? TuKhoa { get; set; }
        public string TrangThai { get; set; } = "tatca";   // tatca | dangmuon | datra | tramuon
        public DateOnly? TuNgay { get; set; }
        public DateOnly? DenNgay { get; set; }

        public int Trang { get; set; } = 1;
        public int KichThuocTrang { get; set; } = 10;
        public int TongSo { get; set; }
        public int TongTrang => Math.Max(1, (int)Math.Ceiling(TongSo / (double)KichThuocTrang));

        public List<MuonItemViewModel> Items { get; set; } = new();
    }

    // Một khoản phạt
    public class PhatItemViewModel
    {
        public int Id { get; set; }
        public string LoaiPhat { get; set; } = "";
        public string? LyDo { get; set; }
        public decimal SoTien { get; set; }
        public bool DaThanhToan { get; set; }
        public DateOnly? NgayThanhToan { get; set; }
        public DateTime NgayTao { get; set; }
        public string TieuDe { get; set; } = "";
        public string MaPhieu { get; set; } = "";

        public string LoaiText => LoaiPhat switch
        {
            "TraMuon" => "Trả muộn",
            "HuHong" => "Hư hỏng",
            "MatSach" => "Mất sách",
            _ => LoaiPhat
        };
    }

    // Tab "Tiền phạt"
    public class PhatViewModel
    {
        public string Loc { get; set; } = "tatca";   // tatca | chuanop | danop
        public decimal TongChuaNop { get; set; }
        public decimal TongDaNop { get; set; }
        public List<PhatItemViewModel> Items { get; set; } = new();
    }
}
