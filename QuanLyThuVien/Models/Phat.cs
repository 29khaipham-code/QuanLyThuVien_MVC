using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class Phat
{
    public int Id { get; set; }

    public int ChiTietMuonId { get; set; }

    public string LoaiPhat { get; set; } = null!;

    public string? LyDo { get; set; }

    public decimal SoTien { get; set; }

    public bool DaThanhToan { get; set; }

    public DateOnly? NgayThanhToan { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual ChiTietMuon ChiTietMuon { get; set; } = null!;
}
