using System.Globalization;

namespace QuanLyThuVien.ViewModels
{
    // Dữ liệu cho partial phân trang dùng chung (_PhanTrang)
    public class PhanTrangViewModel
    {
        public int Trang { get; set; }
        public int TongTrang { get; set; }
        public Func<int, string> Link { get; set; } = _ => "#";
    }

    // Hàm định dạng dùng trong view
    public static class HienThi
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        public static string Tien(decimal soTien) => soTien.ToString("N0", Vi) + " đ";
    }
}
