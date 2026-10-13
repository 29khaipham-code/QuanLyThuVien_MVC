using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class TheLoai
{
    public int Id { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<Sach> Saches { get; set; } = new List<Sach>();
}
