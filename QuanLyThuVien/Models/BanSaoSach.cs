using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class BanSaoSach
{
    public int Id { get; set; }

    public int SachId { get; set; }

    public string MaVach { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public string TinhTrang { get; set; } = null!;

    public DateOnly? NgayNhap { get; set; }

    public string? ViTri { get; set; }

    public decimal? GiaTri { get; set; }

    public virtual ChiTietMuon? ChiTietMuon { get; set; }

    public virtual Sach Sach { get; set; } = null!;

    public virtual ThanhLy? ThanhLy { get; set; }
}
