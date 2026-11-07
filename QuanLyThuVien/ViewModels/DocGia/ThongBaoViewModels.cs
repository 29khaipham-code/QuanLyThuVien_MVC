namespace QuanLyThuVien.ViewModels.DocGia
{
    // Danh sách loại thông báo và cách hiển thị (icon, màu)
    public static class LoaiThongBao
    {
        public const string SapDenHan = "SapDenHan";
        public const string QuaHan = "QuaHan";
        public const string CoSachNhan = "CoSachNhan";
        public const string GiuChoSapHet = "GiuChoSapHet";
        public const string QuyenDocSapHet = "QuyenDocSapHet";
        public const string PhatMoi = "PhatMoi";
        public const string TheSapHetHan = "TheSapHetHan";

        // Dùng cho dropdown lọc
        public static readonly IReadOnlyList<(string Ma, string Ten)> TatCa = new[]
        {
            (SapDenHan,      "Sắp đến hạn trả"),
            (QuaHan,         "Quá hạn trả"),
            (CoSachNhan,     "Sách đặt trước đã sẵn sàng"),
            (GiuChoSapHet,   "Sắp hết hạn giữ chỗ"),
            (QuyenDocSapHet, "Quyền đọc sắp hết hạn"),
            (PhatMoi,        "Tiền phạt"),
            (TheSapHetHan,   "Thẻ sắp hết hạn")
        };

        public static bool HopLe(string? loai) => TatCa.Any(x => x.Ma == loai);

        public static string Icon(string loai) => loai switch
        {
            SapDenHan => "bi-clock",
            QuaHan => "bi-exclamation-triangle",
            CoSachNhan => "bi-bag-check",
            GiuChoSapHet => "bi-hourglass-split",
            QuyenDocSapHet => "bi-tablet",
            PhatMoi => "bi-cash-coin",
            TheSapHetHan => "bi-person-vcard",
            _ => "bi-bell"
        };

        public static string Mau(string loai) => loai switch
        {
            SapDenHan => "text-warning",
            QuaHan => "text-danger",
            CoSachNhan => "text-success",
            GiuChoSapHet => "text-warning",
            QuyenDocSapHet => "text-info",
            PhatMoi => "text-danger",
            TheSapHetHan => "text-warning",
            _ => "text-secondary"
        };
    }

    // Một thông báo
    public class ThongBaoItemViewModel
    {
        public int Id { get; set; }
        public string Loai { get; set; } = "";
        public string TieuDe { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public string? Url { get; set; }
        public bool DaDoc { get; set; }
        public DateTime NgayTao { get; set; }

        public string Icon => LoaiThongBao.Icon(Loai);
        public string Mau => LoaiThongBao.Mau(Loai);

        // Thời gian dạng "5 phút trước"
        public string ThoiGian
        {
            get
            {
                var d = DateTime.Now - NgayTao;
                if (d.TotalMinutes < 1) return "Vừa xong";
                if (d.TotalMinutes < 60) return $"{(int)d.TotalMinutes} phút trước";
                if (d.TotalHours < 24) return $"{(int)d.TotalHours} giờ trước";
                if (d.TotalDays < 7) return $"{(int)d.TotalDays} ngày trước";
                return NgayTao.ToString("dd/MM/yyyy");
            }
        }
    }

    // Trang danh sách thông báo: bộ lọc + phân trang
    public class ThongBaoIndexViewModel
    {
        public string? Loai { get; set; }
        public string TrangThai { get; set; } = "tatca";   // tatca | chuadoc | dadoc

        public int Trang { get; set; } = 1;
        public int KichThuocTrang { get; set; } = 10;
        public int TongSo { get; set; }
        public int TongTrang => Math.Max(1, (int)Math.Ceiling(TongSo / (double)KichThuocTrang));

        public int ChuaDoc { get; set; }
        public List<ThongBaoItemViewModel> Items { get; set; } = new();
    }

    // Quả chuông trên navbar
    public class ChuongViewModel
    {
        public int ChuaDoc { get; set; }
        public List<ThongBaoItemViewModel> Items { get; set; } = new();
    }
}
