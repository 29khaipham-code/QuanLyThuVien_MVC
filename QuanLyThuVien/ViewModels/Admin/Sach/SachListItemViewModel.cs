
namespace QuanLyThuVien.ViewModels.Admin.Sach
{
    public class SachListItemViewModel
    {
        public int Id { get; set; }

        public string? Isbn { get; set; }

        public string TieuDe { get; set; } = "";

        public string TheLoai { get; set; } = "";

        public string NhaXuatBan { get; set; } = "";

        public int? NamXuatBan { get; set; }

        public string? AnhBiaUrl { get; set; }

        public string TrangThai { get; set; } = "";
    }
}
