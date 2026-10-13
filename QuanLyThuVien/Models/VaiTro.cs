using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class VaiTro
{
    public int Id { get; set; }

    public string TenVaiTro { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
