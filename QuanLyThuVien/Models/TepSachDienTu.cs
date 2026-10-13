using System;
using System.Collections.Generic;

namespace QuanLyThuVien.Models;

public partial class TepSachDienTu
{
    public int Id { get; set; }

    public int SachId { get; set; }

    public string DuongDanTep { get; set; } = null!;

    public string DinhDang { get; set; } = null!;

    public long? DungLuong { get; set; }

    public string CheDoTruyCap { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public DateTime NgayTaiLen { get; set; }

    public DateOnly? NgayHetBanQuyen { get; set; }

    public virtual ICollection<LuotDocSachDienTu> LuotDocSachDienTus { get; set; } = new List<LuotDocSachDienTu>();

    public virtual Sach Sach { get; set; } = null!;
}
