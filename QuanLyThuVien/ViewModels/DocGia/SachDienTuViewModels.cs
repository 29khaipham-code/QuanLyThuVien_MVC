namespace QuanLyThuVien.ViewModels.DocGia
{
    // Một quyền đọc sách điện tử của độc giả (một dòng LuotDocSachDienTu)
    public class QuyenDocItemViewModel
    {
        public int LuotDocId { get; set; }
        public int SachId { get; set; }
        public string TieuDe { get; set; } = "";
        public string DinhDang { get; set; } = "";
        public string CheDoTruyCap { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayHetHan { get; set; }
        public DateTime? LanTruyCapCuoi { get; set; }
        public bool TepKhaDung { get; set; }          // tệp còn hoạt động trên hệ thống

        public bool ConHieuLuc => TrangThai == "HieuLuc" && NgayHetHan > DateTime.Now;

        public int SoNgayConLai => Math.Max(0, (int)Math.Ceiling((NgayHetHan - DateTime.Now).TotalDays));

        public bool CoTheXemTrucTuyen => DinhDang.Equals("PDF", StringComparison.OrdinalIgnoreCase);

        public bool CoTheTai => CheDoTruyCap == "ChoPhepTai";

        public string TrangThaiText =>
            ConHieuLuc ? "Đang có quyền đọc"
            : TrangThai == "ThuHoi" ? "Đã bị thu hồi"
            : "Đã hết hạn";
    }

    // Tab "Sách của tôi"
    public class SachCuaToiViewModel
    {
        public List<QuyenDocItemViewModel> DangHieuLuc { get; set; } = new();
        public List<QuyenDocItemViewModel> LichSu { get; set; } = new();
        public int ToiDa { get; set; }
        public int SoNgayDoc { get; set; }
    }

    // Trang đọc sách
    public class DocSachViewModel
    {
        public int LuotDocId { get; set; }
        public int SachId { get; set; }
        public string TieuDe { get; set; } = "";
        public string DinhDang { get; set; } = "";
        public bool CoTheXemTrucTuyen { get; set; }
        public bool CoTheTai { get; set; }
        public DateTime NgayHetHan { get; set; }
        public string TokenXem { get; set; } = "";    // token ngắn hạn dùng để mở tệp trong khung đọc

        public int SoNgayConLai => Math.Max(0, (int)Math.Ceiling((NgayHetHan - DateTime.Now).TotalDays));
    }
}
