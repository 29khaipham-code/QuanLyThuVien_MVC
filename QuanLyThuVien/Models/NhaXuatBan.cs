using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class NhaXuatBan
{
    public int Id { get; set; }

    public string TenNhaXuatBan { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public virtual ICollection<Sach> Saches { get; set; } = new List<Sach>();
}
