using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class LuotDocSachDienTu
{
    public int Id { get; set; }

    public int DocGiaId { get; set; }

    public int TepSachId { get; set; }

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayHetHan { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime? LanTruyCapCuoi { get; set; }

    public virtual DocGium DocGia { get; set; } = null!;

    public virtual ICollection<NhatKyTruyCap> NhatKyTruyCaps { get; set; } = new List<NhatKyTruyCap>();

    public virtual TepSachDienTu TepSach { get; set; } = null!;
}
