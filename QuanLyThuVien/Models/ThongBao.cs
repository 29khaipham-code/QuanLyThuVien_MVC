using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class ThongBao
{
    public int Id { get; set; }

    public int DocGiaId { get; set; }

    public string Loai { get; set; } = null!;

    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public string? Url { get; set; }

    public string KhoaTrung { get; set; } = null!;

    public bool DaDoc { get; set; }

    public bool DaXoa { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayDoc { get; set; }

    public virtual DocGium DocGia { get; set; } = null!;
}
