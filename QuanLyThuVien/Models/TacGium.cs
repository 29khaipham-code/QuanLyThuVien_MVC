using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class TacGium
{
    public int Id { get; set; }

    public string TenTacGia { get; set; } = null!;

    public string? TieuSu { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? QuocTich { get; set; }

    public virtual ICollection<Sach> Saches { get; set; } = new List<Sach>();
}
