using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class PhieuMuon
{
    public int Id { get; set; }

    public string MaPhieu { get; set; } = null!;

    public int DocGiaId { get; set; }

    public int NhanVienLapId { get; set; }

    public DateOnly NgayMuon { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual ICollection<ChiTietMuon> ChiTietMuons { get; set; } = new List<ChiTietMuon>();

    public virtual DocGium DocGia { get; set; } = null!;

    public virtual TaiKhoan NhanVienLap { get; set; } = null!;
}
