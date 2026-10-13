using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class ChiTietMuon
{
    public int Id { get; set; }

    public int PhieuMuonId { get; set; }

    public int BanSaoId { get; set; }

    public DateOnly HanTra { get; set; }

    public DateTime? NgayTra { get; set; }

    public string TinhTrangTruoc { get; set; } = null!;

    public string? TinhTrangSau { get; set; }

    public string? GhiChu { get; set; }

    public virtual BanSaoSach BanSao { get; set; } = null!;

    public virtual Phat? Phat { get; set; }

    public virtual PhieuMuon PhieuMuon { get; set; } = null!;
}
