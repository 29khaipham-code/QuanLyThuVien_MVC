
namespace QuanLyThuVien.ViewModels.DatTruoc
{
    public class DatTruocListItemViewModel
    {
        public int Id { get; set; }
        public int DocGiaId { get; set; }
        public int SachId { get; set; }

        public string MaDocGia { get; set; } = "";
        public string HoTen { get; set; } = "";
        public string TenSach { get; set; } = "";

        public DateTime NgayDat { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public int? BanSaoGiuId { get; set; }

        public string? MaVach { get; set; }
        public string TrangThai { get; set; } = "";
    }
}
