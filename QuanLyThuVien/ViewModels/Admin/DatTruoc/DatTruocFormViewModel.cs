
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.ViewModels.DatTruoc
{
    public class DatTruocFormViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn độc giả")]
        [Display(Name = "Độc giả")]
        public int DocGiaId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn sách")]
        [Display(Name = "Sách đặt trước")]
        public int SachId { get; set; }
    }
}
