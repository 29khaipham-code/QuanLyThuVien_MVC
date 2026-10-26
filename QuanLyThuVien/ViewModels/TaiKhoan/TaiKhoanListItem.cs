
namespace QuanLyThuVien.ViewModels.TaiKhoan
{
    public class TaiKhoanListItem
    {
        public int Id { get; set; }

        public string TenDangNhap { get; set; } = "";

        public string HoTen { get; set; } = "";

        public string Email { get; set; } = "";

        public string TenVaiTro { get; set; } = "";

        public bool HoatDong { get; set; }

        public DateTime NgayTao { get; set; }
    }
}
