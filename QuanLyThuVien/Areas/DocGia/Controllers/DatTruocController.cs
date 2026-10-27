using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Models;

namespace QuanLyThuVien.Areas.DocGia.Controllers
{
    public class DatTruocController : DocGiaBaseController
    {
        public DatTruocController(QuanLyThuVienContext db) : base(db) { }
        public IActionResult Index() => View();
    }
}