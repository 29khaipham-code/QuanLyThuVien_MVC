using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyThuVien.Controllers
{
    [Authorize(Roles = "QuanTri,ThuThu")]
    public class PhieuMuonController : Controller
    {
        // Các chức năng mượn trả
    }
}
