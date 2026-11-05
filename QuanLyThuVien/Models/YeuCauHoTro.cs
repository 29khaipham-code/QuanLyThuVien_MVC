
using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien.Models
{
    public class YeuCauHoTro
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ChuDe { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string LoiNhan { get; set; } = string.Empty;

        public DateTime NgayGui { get; set; } = DateTime.Now;
    }
}
