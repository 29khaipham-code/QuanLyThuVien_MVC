using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class DocGium
{
    public int Id { get; set; }

    public string MaDocGia { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public DateOnly? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? Email { get; set; }

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public DateOnly NgayDangKy { get; set; }

    public DateOnly NgayHetHan { get; set; }

    public string TrangThai { get; set; } = null!;

    public int? TaiKhoanId { get; set; }

    public int LoaiDocGiaId { get; set; }

    public virtual ICollection<DatTruoc> DatTruocs { get; set; } = new List<DatTruoc>();

    public virtual LoaiDocGium LoaiDocGia { get; set; } = null!;

    public virtual ICollection<LuotDocSachDienTu> LuotDocSachDienTus { get; set; } = new List<LuotDocSachDienTu>();

    public virtual ICollection<PhieuMuon> PhieuMuons { get; set; } = new List<PhieuMuon>();

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
