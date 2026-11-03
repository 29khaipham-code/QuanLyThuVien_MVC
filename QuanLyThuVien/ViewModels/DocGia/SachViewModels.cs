namespace QuanLyThuVien.ViewModels.DocGia
{
    // Một mục trong dropdown lọc (thể loại, NXB)
    public class LuaChonViewModel
    {
        public int Id { get; set; }
        public string Ten { get; set; } = "";
    }

    // Một cuốn sách trong danh sách
    public class SachItemViewModel
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = "";
        public string? AnhBiaUrl { get; set; }
        public string TheLoai { get; set; } = "";
        public string NhaXuatBan { get; set; } = "";
        public int? NamXuatBan { get; set; }
        public List<string> TacGias { get; set; } = new();

        public int TongBanSao { get; set; }   // không tính bản đã thanh lý
        public int SanSang { get; set; }
        public bool CoSachDienTu { get; set; }

        public string TacGia => TacGias.Count == 0 ? "Chưa rõ tác giả" : string.Join(", ", TacGias);
    }

    // Trang danh sách: bộ lọc + kết quả + phân trang
    public class SachIndexViewModel
    {
        // Bộ lọc
        public string? TuKhoa { get; set; }
        public int? TheLoaiId { get; set; }
        public int? NhaXuatBanId { get; set; }
        public bool ConSach { get; set; }
        public string SapXep { get; set; } = "moi";

        // Phân trang
        public int Trang { get; set; } = 1;
        public int KichThuocTrang { get; set; } = 8;
        public int TongSo { get; set; }
        public int TongTrang => (int)Math.Ceiling(TongSo / (double)KichThuocTrang);

        // Dữ liệu hiển thị
        public List<SachItemViewModel> Items { get; set; } = new();
        public List<LuaChonViewModel> TheLoais { get; set; } = new();
        public List<LuaChonViewModel> NhaXuatBans { get; set; } = new();
    }

    // Trang chi tiết sách
    public class SachChiTietViewModel
    {
        public int Id { get; set; }
        public string? Isbn { get; set; }
        public string TieuDe { get; set; } = "";
        public string? AnhBiaUrl { get; set; }
        public string? MoTa { get; set; }
        public string TheLoai { get; set; } = "";
        public string NhaXuatBan { get; set; } = "";
        public int? NamXuatBan { get; set; }
        public int? LanTaiBan { get; set; }
        public List<string> TacGias { get; set; } = new();
        public List<string> DinhDangs { get; set; } = new();

        // Số lượng (lấy từ vw_SoLuongSach)
        public int TongBanSao { get; set; }
        public int SanSang { get; set; }
        public int DangMuon { get; set; }
        public int DaDat { get; set; }

        // Trạng thái của độc giả đang xem
        public bool CoHoSo { get; set; }
        public bool DangMuonCuonNay { get; set; }
        public string? DatTruocTrangThai { get; set; }   // "DangCho", "DaGiu" hoặc null
        public int ViTriHangDoi { get; set; }            // vị trí trong hàng đợi (khi DangCho)

        public bool CoSachDienTu => DinhDangs.Count > 0;
        public string TacGia => TacGias.Count == 0 ? "Chưa rõ tác giả" : string.Join(", ", TacGias);
    }
}
