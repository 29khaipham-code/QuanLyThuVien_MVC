using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class NhatKyTruyCap
{
    public long Id { get; set; }

    public int LuotDocId { get; set; }

    public DateTime ThoiDiem { get; set; }

    public string HanhDong { get; set; } = null!;

    public string? DiaChiIp { get; set; }

    public virtual LuotDocSachDienTu LuotDoc { get; set; } = null!;
}
