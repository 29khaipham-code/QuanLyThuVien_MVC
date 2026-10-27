using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class DatTruoc
{
    public int Id { get; set; }

    public int DocGiaId { get; set; }

    public int SachId { get; set; }

    public DateTime NgayDat { get; set; }

    public DateTime? NgayHetHan { get; set; }

    public int? BanSaoGiuId { get; set; }

    public string TrangThai { get; set; } = null!;

    public virtual BanSaoSach? BanSaoGiu { get; set; }

    public virtual DocGium DocGia { get; set; } = null!;

    public virtual Sach Sach { get; set; } = null!;
}
