using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class LoaiDocGium
{
    public int Id { get; set; }

    public string TenLoai { get; set; } = null!;

    public int SoSachToiDa { get; set; }

    public int SoNgayMuonToiDa { get; set; }

    public string? MoTa { get; set; }

    public virtual ICollection<DocGium> DocGia { get; set; } = new List<DocGium>();
}
