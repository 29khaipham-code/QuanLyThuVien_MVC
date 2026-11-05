
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuanLyThuVien.ViewModels.Admin.SachDienTu
{
    public class TepSachItemVm
    {
        public int Id { get; set; }
        public int SachId { get; set; }
        public string TieuDe { get; set; } = "";
        public string DinhDang { get; set; } = "";
        public long? DungLuong { get; set; }
        public string CheDoTruyCap { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public DateTime NgayTaiLen { get; set; }
        public DateTime? NgayHetBanQuyen { get; set; }
    }

    public class SachDienTuIndexVm
    {
        public string? TuKhoa { get; set; }
        public string? TrangThai { get; set; }
        public int Trang { get; set; } = 1;
        public int Tong { get; set; }

        public int SoTrang =>
            Math.Max(1, (Tong + 9) / 10);

        public List<TepSachItemVm> DanhSach { get; set; }
            = new();
    }

    public class TepSachFormVm
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Vui lòng chọn đầu sách")]
        public int SachId { get; set; }

        [Required]
        [RegularExpression("^(DocTrucTuyen|ChoPhepTai)$")]
        public string CheDoTruyCap { get; set; }
            = "DocTrucTuyen";

        [Required]
        [RegularExpression("^(HoatDong|TamKhoa|HetBanQuyen)$")]
        public string TrangThai { get; set; }
            = "HoatDong";

        [DataType(DataType.Date)]
        public DateTime? NgayHetBanQuyen { get; set; }

        public IFormFile? Tep { get; set; }

        public string? TenTepHienTai { get; set; }

        public List<SelectListItem> DauSach { get; set; }
            = new();
    }

    public class QuyenDocVm
    {
        public int Id { get; set; }
        public string DocGia { get; set; } = "";
        public string TenSach { get; set; } = "";
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayHetHan { get; set; }
        public string TrangThai { get; set; } = "";
    }

    public class CapQuyenVm
    {
        [Range(1, int.MaxValue)]
        public int DocGiaId { get; set; }

        [Range(1, int.MaxValue)]
        public int TepSachId { get; set; }

        [Range(1, 365)]
        public int SoNgay { get; set; } = 7;
    }

    public class NhatKyVm
    {
        public long Id { get; set; }
        public string DocGia { get; set; } = "";
        public string TenSach { get; set; } = "";
        public string HanhDong { get; set; } = "";
        public DateTime ThoiDiem { get; set; }
        public string? DiaChiIP { get; set; }
    }
}
