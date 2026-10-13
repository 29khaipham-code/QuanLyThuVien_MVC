using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class VwSoLuongSach
{
    public int SachId { get; set; }

    public string TieuDe { get; set; } = null!;

    public int? TongBanSao { get; set; }

    public int? SanSang { get; set; }

    public int? DangMuon { get; set; }

    public int? DaDat { get; set; }

    public int? HuHong { get; set; }

    public int? Mat { get; set; }

    public int? DangSua { get; set; }

    public int? DaThanhLy { get; set; }
}
