
using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Models
{
    public class DangKyNhanTin
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public DateTime NgayDangKy { get; set; } = DateTime.Now;
    }
}
