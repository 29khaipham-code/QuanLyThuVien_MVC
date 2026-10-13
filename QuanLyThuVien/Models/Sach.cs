using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class Sach
{
    public int Id { get; set; }

    public string? Isbn { get; set; }

    public string TieuDe { get; set; } = null!;

    public int TheLoaiId { get; set; }

    public int NhaXuatBanId { get; set; }

    public int? NamXuatBan { get; set; }

    public int? LanTaiBan { get; set; }

    public string? MoTa { get; set; }

    public string? AnhBiaUrl { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<BanSaoSach> BanSaoSaches { get; set; } = new List<BanSaoSach>();

    public virtual NhaXuatBan NhaXuatBan { get; set; } = null!;

    public virtual ICollection<TepSachDienTu> TepSachDienTus { get; set; } = new List<TepSachDienTu>();

    public virtual TheLoai TheLoai { get; set; } = null!;

    public virtual ICollection<TacGium> TacGia { get; set; } = new List<TacGium>();
}
