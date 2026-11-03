using QuanLyThuVien.ViewModels.DocGia;

namespace QuanLyThuVien.ViewModels.TrangChu
{
    // Một slide của carousel
    public class BannerViewModel
    {
        public string TieuDe { get; set; } = "";
        public string MoTa { get; set; } = "";
        public string NutChu { get; set; } = "";
        public string Url { get; set; } = "#";
        public string LopNen { get; set; } = "bg-primary";   // lớp nền Bootstrap khi chưa có ảnh
        public string? AnhUrl { get; set; }                  // ví dụ: images/banner/b1.jpg (tùy chọn)
    }

    // Một kệ sách (nhóm sách) trên trang chủ
    public class KeSachViewModel
    {
        public string TieuDe { get; set; } = "";
        public string? MoTa { get; set; }
        public string? Icon { get; set; }       // tên icon Bootstrap Icons, ví dụ "bi-stars"
        public string? Url { get; set; }        // link "Xem tất cả", có thể để trống
        public List<SachItemViewModel> Items { get; set; } = new();
    }

    public class TheLoaiThongKeViewModel
    {
        public int Id { get; set; }
        public string Ten { get; set; } = "";
        public int SoSach { get; set; }
    }

    public class TrangChuViewModel
    {
        // Carousel
        public List<BannerViewModel> Banners { get; set; } = new();

        // Thông tin độc giả
        public bool CoHoSo { get; set; }
        public string HoTen { get; set; } = "";
        public string MaDocGia { get; set; } = "";
        public string LoaiDocGia { get; set; } = "";
        public DateOnly NgayHetHan { get; set; }
        public string TrangThai { get; set; } = "";
        public int SoSachToiDa { get; set; }

        // Thống kê mượn trả
        public int DangMuon { get; set; }
        public int SapDenHan { get; set; }      // hạn trả trong 3 ngày tới
        public int QuaHan { get; set; }
        public decimal PhatChuaThanhToan { get; set; }

        // Các kệ sách: Mới nhất, Gợi ý, Đã đọc, Sách điện tử
        public List<KeSachViewModel> KeSach { get; set; } = new();

        // Theo chủ đề: danh sách chip + vài kệ theo thể loại
        public List<TheLoaiThongKeViewModel> TheLoais { get; set; } = new();
        public List<KeSachViewModel> KeTheoChuDe { get; set; } = new();

        public bool TheConHieuLuc =>
            TrangThai == "HoatDong" && NgayHetHan >= DateOnly.FromDateTime(DateTime.Today);
    }
}
