using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class TaiKhoan
{
    public int Id { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public bool HoatDong { get; set; }

    public DateTime NgayTao { get; set; }

    public int VaiTroId { get; set; }

    public virtual DocGium? DocGium { get; set; }

    public virtual ICollection<PhieuMuon> PhieuMuons { get; set; } = new List<PhieuMuon>();

    public virtual ICollection<ThanhLy> ThanhLies { get; set; } = new List<ThanhLy>();

    public virtual VaiTro VaiTro { get; set; } = null!;
}
