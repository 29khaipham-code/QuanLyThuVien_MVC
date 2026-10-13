using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class ThanhLy
{
    public int Id { get; set; }

    public int BanSaoId { get; set; }

    public string LyDo { get; set; } = null!;

    public DateOnly NgayThanhLy { get; set; }

    public int NhanVienId { get; set; }

    public virtual BanSaoSach BanSao { get; set; } = null!;

    public virtual TaiKhoan NhanVien { get; set; } = null!;
}
