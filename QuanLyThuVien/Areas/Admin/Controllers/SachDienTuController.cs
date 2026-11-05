
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Services;
using QuanLyThuVien.ViewModels.Admin.SachDienTu;

namespace QuanLyThuVien.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "QuanTri")]
    public class SachDienTuController : Controller
    {
        private readonly SachDienTuAdminService _service;

        public SachDienTuController(
            SachDienTuAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? tuKhoa, string? trangThai, int trang = 1)
        {
            return View(await _service.LayDanhSach(
                tuKhoa, trangThai, trang));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View(new TepSachFormVm
            {
                DauSach = await _service.LayDauSach()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(26L * 1024 * 1024)]
        public async Task<IActionResult> Create(TepSachFormVm vm)
        {
            if (vm.Tep == null)
                ModelState.AddModelError(
                    nameof(vm.Tep), "Hãy chọn tệp PDF.");

            if (vm.NgayHetBanQuyen.HasValue &&
                vm.NgayHetBanQuyen.Value.Date < DateTime.Today)
                ModelState.AddModelError(
                    nameof(vm.NgayHetBanQuyen),
                    "Ngày hết bản quyền không hợp lệ.");

            if (ModelState.IsValid)
            {
                try
                {
                    await _service.Them(vm);
                    TempData["Success"] = "Thêm sách điện tử thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            vm.DauSach = await _service.LayDauSach();
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _service.LayTep(id);

            if (vm == null)
                return NotFound();

            vm.DauSach = await _service.LayDauSach();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TepSachFormVm vm)
        {
            if (vm.NgayHetBanQuyen.HasValue &&
                vm.NgayHetBanQuyen.Value.Date < DateTime.Today &&
                vm.TrangThai == "HoatDong")
            {
                ModelState.AddModelError(
                    nameof(vm.NgayHetBanQuyen),
                    "Bản quyền đã hết hạn.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _service.Sua(vm);
                    TempData["Success"] = "Cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            vm.DauSach = await _service.LayDauSach();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(26L * 1024 * 1024)]
        public async Task<IActionResult> Replace(
            int id, IFormFile? tep)
        {
            if (tep == null)
            {
                TempData["Error"] = "Chưa chọn tệp PDF.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            try
            {
                await _service.ThayTep(id, tep);
                TempData["Success"] = "Thay PDF thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(
            int id, bool mo)
        {
            try
            {
                await _service.DoiTrangThai(id, mo);
                TempData["Success"] = "Đổi trạng thái thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> QuyenDoc()
        {
            return View(await _service.LayQuyenDoc());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapQuyen(CapQuyenVm vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu cấp quyền không hợp lệ.";
                return RedirectToAction(nameof(QuyenDoc));
            }

            try
            {
                await _service.CapQuyen(vm);
                TempData["Success"] = "Cấp quyền thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(QuyenDoc));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaHan(int id, int soNgay = 7)
        {
            try
            {
                await _service.GiaHan(id, soNgay);
                TempData["Success"] = "Gia hạn thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(QuyenDoc));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThuHoi(int id)
        {
            try
            {
                await _service.ThuHoi(id);
                TempData["Success"] = "Đã thu hồi quyền đọc.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(QuyenDoc));
        }

        [HttpGet]
        public async Task<IActionResult> NhatKy()
        {
            return View(await _service.LayNhatKy());
        }
    }
}
