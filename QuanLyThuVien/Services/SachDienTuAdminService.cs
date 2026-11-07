
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using QuanLyThuVien.ViewModels.Admin.SachDienTu;

namespace QuanLyThuVien.Services
{
    public class SachDienTuAdminService
    {
        private readonly string _connectionString;
        private readonly string _storagePath;
        private const long MaxPdfBytes = 25L * 1024 * 1024;

        public SachDienTuAdminService(
            IConfiguration config,
            IWebHostEnvironment env)
        {
            _connectionString = config.GetConnectionString(
                "QuanLyThuVienConnectionString")
                ?? throw new InvalidOperationException(
                    "Thiếu chuỗi kết nối QuanLyThuVienConnectionString");

            _storagePath = Path.Combine(
                env.ContentRootPath, "App_Data", "ebooks");
        }

        private SqlConnection OpenConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private static void Add(
            SqlCommand cmd,
            string name,
            SqlDbType type,
            object? value,
            int size = 0)
        {
            var parameter = size > 0
                ? cmd.Parameters.Add(name, type, size)
                : cmd.Parameters.Add(name, type);

            parameter.Value = value ?? DBNull.Value;
        }

        // DANH SACH, TIM KIEM, PHAN TRANG
        public async Task<SachDienTuIndexVm> LayDanhSach(
            string? tuKhoa, string? trangThai, int trang)
        {
            trang = Math.Max(1, trang);
            tuKhoa = (tuKhoa ?? "").Trim();

            if (trangThai != "HoatDong" &&
                trangThai != "TamKhoa" &&
                trangThai != "HetBanQuyen")
            {
                trangThai = null;
            }

            var vm = new SachDienTuIndexVm
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                Trang = trang
            };

            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string where = @"
                WHERE (@q = N'' OR s.TieuDe LIKE N'%' + @q + N'%')
                AND (@st IS NULL OR t.TrangThai = @st)";

            await using (var cmd = new SqlCommand(
                @"SELECT COUNT(*)
                  FROM dbo.TepSachDienTu t
                  JOIN dbo.Sach s ON s.Id = t.SachId "
                + where, conn))
            {
                Add(cmd, "@q", SqlDbType.NVarChar, tuKhoa, 255);
                Add(cmd, "@st", SqlDbType.NVarChar, trangThai, 30);

                vm.Tong = Convert.ToInt32(
                    await cmd.ExecuteScalarAsync());
            }

            vm.Trang = Math.Min(vm.Trang, vm.SoTrang);

            string sql = @"
                SELECT t.Id, t.SachId, s.TieuDe,
                       t.DinhDang, t.DungLuong,
                       t.CheDoTruyCap, t.TrangThai,
                       t.NgayTaiLen, t.NgayHetBanQuyen
                FROM dbo.TepSachDienTu t
                JOIN dbo.Sach s ON s.Id = t.SachId "
                + where + @"
                ORDER BY t.Id DESC
                OFFSET @skip ROWS
                FETCH NEXT 10 ROWS ONLY";

            await using var listCmd = new SqlCommand(sql, conn);

            Add(listCmd, "@q", SqlDbType.NVarChar, tuKhoa, 255);
            Add(listCmd, "@st", SqlDbType.NVarChar, trangThai, 30);
            Add(listCmd, "@skip", SqlDbType.Int, (vm.Trang - 1) * 10);

            await using var rd = await listCmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                vm.DanhSach.Add(new TepSachItemVm
                {
                    Id = rd.GetInt32(0),
                    SachId = rd.GetInt32(1),
                    TieuDe = rd.GetString(2),
                    DinhDang = rd.GetString(3),
                    DungLuong = rd.IsDBNull(4)
                        ? null : rd.GetInt64(4),
                    CheDoTruyCap = rd.GetString(5),
                    TrangThai = rd.GetString(6),
                    NgayTaiLen = rd.GetDateTime(7),
                    NgayHetBanQuyen = rd.IsDBNull(8)
                        ? null : rd.GetDateTime(8)
                });
            }

            return vm;
        }

        // LAY DAU SACH CHO DROPDOWN
        public async Task<List<SelectListItem>> LayDauSach()
        {
            var result = new List<SelectListItem>();

            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT Id, TieuDe
                FROM dbo.Sach
                WHERE TrangThai = N'HoatDong'
                ORDER BY TieuDe";

            await using var cmd = new SqlCommand(sql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                result.Add(new SelectListItem
                {
                    Value = rd.GetInt32(0).ToString(),
                    Text = rd.GetString(1)
                });
            }

            return result;
        }

        // LAY THONG TIN 1 TEP
        public async Task<TepSachFormVm?> LayTep(int id)
        {
            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT Id, SachId, CheDoTruyCap,
                       TrangThai, NgayHetBanQuyen, DuongDanTep
                FROM dbo.TepSachDienTu
                WHERE Id = @id";

            await using var cmd = new SqlCommand(sql, conn);
            Add(cmd, "@id", SqlDbType.Int, id);

            await using var rd = await cmd.ExecuteReaderAsync();

            if (!await rd.ReadAsync())
                return null;

            return new TepSachFormVm
            {
                Id = rd.GetInt32(0),
                SachId = rd.GetInt32(1),
                CheDoTruyCap = rd.GetString(2),
                TrangThai = rd.GetString(3),
                NgayHetBanQuyen = rd.IsDBNull(4)
                    ? null : rd.GetDateTime(4),
                TenTepHienTai =
                    Path.GetFileName(rd.GetString(5))
            };
        }

        // KIEM TRA FILE PDF
        private static async Task ValidatePdf(IFormFile file)
        {
            if (file.Length == 0 ||
                file.Length > MaxPdfBytes ||
                !string.Equals(
                    Path.GetExtension(file.FileName),
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Chỉ nhận PDF tối đa 25 MB.");
            }

            byte[] header = new byte[5];

            await using var stream = file.OpenReadStream();

            int read = await stream.ReadAsync(
                header, 0, header.Length);

            if (read != 5 ||
                System.Text.Encoding.ASCII.GetString(header)
                    != "%PDF-")
            {
                throw new InvalidOperationException(
                    "Tệp không phải PDF hợp lệ.");
            }
        }

        // THEM SACH DIEN TU
        public async Task Them(TepSachFormVm vm)
        {
            if (vm.Tep == null)
                throw new InvalidOperationException(
                    "Chưa chọn PDF.");

            await ValidatePdf(vm.Tep);

            Directory.CreateDirectory(_storagePath);

            string fileName = Guid.NewGuid().ToString("N") + ".pdf";
            string fullPath = Path.Combine(_storagePath, fileName);

            await using (var file = File.Create(fullPath))
            {
                await vm.Tep.CopyToAsync(file);
            }

            try
            {
                await using var conn = OpenConnection();
                await conn.OpenAsync();

                const string sql = @"
                    INSERT INTO dbo.TepSachDienTu
                    (SachId, DuongDanTep, DinhDang,
                     DungLuong, CheDoTruyCap,
                     TrangThai, NgayHetBanQuyen)
                    VALUES
                    (@s, @p, 'PDF', @len, @cd, @st, @han)";

                await using var cmd = new SqlCommand(sql, conn);

                Add(cmd, "@s", SqlDbType.Int, vm.SachId);
                Add(cmd, "@p", SqlDbType.NVarChar, fileName, 500);
                Add(cmd, "@len", SqlDbType.BigInt, vm.Tep.Length);
                Add(cmd, "@cd", SqlDbType.NVarChar,
                    vm.CheDoTruyCap, 30);
                Add(cmd, "@st", SqlDbType.NVarChar,
                    vm.TrangThai, 30);
                Add(cmd, "@han", SqlDbType.Date,
                    vm.NgayHetBanQuyen);

                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                File.Delete(fullPath);
                throw;
            }
        }

        // SUA THONG TIN
        public async Task Sua(TepSachFormVm vm)
        {
            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                UPDATE dbo.TepSachDienTu
                SET CheDoTruyCap = @cd,
                    TrangThai = @st,
                    NgayHetBanQuyen = @han
                WHERE Id = @id";

            await using var cmd = new SqlCommand(sql, conn);

            Add(cmd, "@id", SqlDbType.Int, vm.Id);
            Add(cmd, "@cd", SqlDbType.NVarChar,
                vm.CheDoTruyCap, 30);
            Add(cmd, "@st", SqlDbType.NVarChar,
                vm.TrangThai, 30);
            Add(cmd, "@han", SqlDbType.Date,
                vm.NgayHetBanQuyen);

            if (await cmd.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException(
                    "Không tìm thấy tệp sách.");
        }

        // THAY THE PDF
        public async Task ThayTep(int id, IFormFile file)
        {
            await ValidatePdf(file);

            Directory.CreateDirectory(_storagePath);

            string fileName = Guid.NewGuid().ToString("N") + ".pdf";
            string fullPath = Path.Combine(_storagePath, fileName);

            await using (var dest = File.Create(fullPath))
            {
                await file.CopyToAsync(dest);
            }

            try
            {
                await using var conn = OpenConnection();
                await conn.OpenAsync();

                const string sql = @"
                    UPDATE dbo.TepSachDienTu
                    SET DuongDanTep = @p,
                        DinhDang = 'PDF',
                        DungLuong = @len,
                        NgayTaiLen = SYSDATETIME()
                    WHERE Id = @id";

                await using var cmd = new SqlCommand(sql, conn);

                Add(cmd, "@p", SqlDbType.NVarChar, fileName, 500);
                Add(cmd, "@len", SqlDbType.BigInt, file.Length);
                Add(cmd, "@id", SqlDbType.Int, id);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new InvalidOperationException(
                        "Không tìm thấy tệp sách.");
            }
            catch
            {
                File.Delete(fullPath);
                throw;
            }
        }

        // KHOA / MO TEP
        public async Task DoiTrangThai(int id, bool mo)
        {
            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                UPDATE dbo.TepSachDienTu
                SET TrangThai = @st
                WHERE Id = @id
                  AND (
                    @st = N'TamKhoa'
                    OR NgayHetBanQuyen IS NULL
                    OR NgayHetBanQuyen >=
                       CAST(SYSDATETIME() AS DATE)
                  )";

            await using var cmd = new SqlCommand(sql, conn);

            Add(cmd, "@st", SqlDbType.NVarChar,
                mo ? "HoatDong" : "TamKhoa", 30);
            Add(cmd, "@id", SqlDbType.Int, id);

            if (await cmd.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException(
                    "Không tìm thấy tệp hoặc bản quyền đã hết.");
        }

        // DANH SACH QUYEN DOC
        public async Task<List<QuyenDocVm>> LayQuyenDoc()
        {
            var result = new List<QuyenDocVm>();

            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT TOP (200)
                    l.Id, d.HoTen, s.TieuDe,
                    l.NgayBatDau, l.NgayHetHan, l.TrangThai
                FROM dbo.LuotDocSachDienTu l
                JOIN dbo.DocGia d ON d.Id = l.DocGiaId
                JOIN dbo.TepSachDienTu t ON t.Id = l.TepSachId
                JOIN dbo.Sach s ON s.Id = t.SachId
                ORDER BY l.Id DESC";

            await using var cmd = new SqlCommand(sql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                result.Add(new QuyenDocVm
                {
                    Id = rd.GetInt32(0),
                    DocGia = rd.GetString(1),
                    TenSach = rd.GetString(2),
                    NgayBatDau = rd.GetDateTime(3),
                    NgayHetHan = rd.GetDateTime(4),
                    TrangThai = rd.GetString(5)
                });
            }

            return result;
        }

        // CAP QUYEN DOC
        public async Task CapQuyen(CapQuyenVm vm)
        {
            if (vm.SoNgay < 1 || vm.SoNgay > 365)
                throw new InvalidOperationException(
                    "Số ngày phải từ 1 đến 365.");

            await using var conn = OpenConnection();
            await conn.OpenAsync();

            await using var tx = (SqlTransaction)
                await conn.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                const string closeSql = @"
                    UPDATE dbo.LuotDocSachDienTu
                    SET TrangThai = N'HetHan'
                    WHERE DocGiaId = @d
                    AND TepSachId = @t
                    AND TrangThai = N'HieuLuc'
                    AND NgayHetHan <= SYSDATETIME()";

                await using (var close =
                    new SqlCommand(closeSql, conn, tx))
                {
                    Add(close, "@d", SqlDbType.Int, vm.DocGiaId);
                    Add(close, "@t", SqlDbType.Int, vm.TepSachId);
                    await close.ExecuteNonQueryAsync();
                }

                const string sql = @"
                    IF NOT EXISTS (
                        SELECT 1
                        FROM dbo.LuotDocSachDienTu
                            WITH (UPDLOCK, HOLDLOCK)
                        WHERE DocGiaId = @d
                        AND TepSachId = @t
                        AND TrangThai = N'HieuLuc'
                    )
                    AND EXISTS (
                        SELECT 1
                        FROM dbo.DocGia d
                        JOIN dbo.TaiKhoan a
                          ON a.Id = d.TaiKhoanId
                        WHERE d.Id = @d
                          AND d.TrangThai = N'HoatDong'
                          AND d.NgayHetHan >=
                            CAST(SYSDATETIME() AS DATE)
                          AND a.HoatDong = 1
                    )
                    AND EXISTS (
                        SELECT 1
                        FROM dbo.TepSachDienTu t
                        JOIN dbo.Sach s ON s.Id = t.SachId
                        WHERE t.Id = @t
                          AND t.TrangThai = N'HoatDong'
                          AND s.TrangThai = N'HoatDong'
                          AND (
                              t.NgayHetBanQuyen IS NULL
                              OR t.NgayHetBanQuyen >=
                                CAST(SYSDATETIME() AS DATE)
                          )
                    )
                    BEGIN
                        INSERT INTO dbo.LuotDocSachDienTu
                            (DocGiaId, TepSachId, NgayHetHan)
                        VALUES (
                            @d, @t,
                            DATEADD(DAY, @days, SYSDATETIME())
                        );
                        SELECT 1;
                    END
                    ELSE SELECT 0;";

                await using (var cmd =
                    new SqlCommand(sql, conn, tx))
                {
                    Add(cmd, "@d", SqlDbType.Int, vm.DocGiaId);
                    Add(cmd, "@t", SqlDbType.Int, vm.TepSachId);
                    Add(cmd, "@days", SqlDbType.Int, vm.SoNgay);

                    int success = Convert.ToInt32(
                        await cmd.ExecuteScalarAsync());

                    if (success != 1)
                        throw new InvalidOperationException(
                            "Độc giả/tệp không hợp lệ hoặc quyền còn hiệu lực.");
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // GIA HAN QUYEN DOC
        public async Task GiaHan(int id, int soNgay)
        {
            if (soNgay < 1 || soNgay > 365)
                throw new InvalidOperationException(
                    "Số ngày gia hạn không hợp lệ.");

            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                UPDATE l
                SET NgayHetHan = DATEADD(
                    DAY, @days,
                    CASE
                        WHEN l.NgayHetHan > SYSDATETIME()
                        THEN l.NgayHetHan
                        ELSE SYSDATETIME()
                    END)
                FROM dbo.LuotDocSachDienTu l
                JOIN dbo.TepSachDienTu t
                    ON t.Id = l.TepSachId
                JOIN dbo.Sach s ON s.Id = t.SachId
                JOIN dbo.DocGia d ON d.Id = l.DocGiaId
                WHERE l.Id = @id
                  AND l.TrangThai = N'HieuLuc'
                  AND t.TrangThai = N'HoatDong'
                  AND s.TrangThai = N'HoatDong'
                  AND d.TrangThai = N'HoatDong'
                  AND d.NgayHetHan >=
                    CAST(SYSDATETIME() AS DATE)
                  AND (
                    t.NgayHetBanQuyen IS NULL
                    OR t.NgayHetBanQuyen >=
                      CAST(SYSDATETIME() AS DATE)
                  )";

            await using var cmd = new SqlCommand(sql, conn);

            Add(cmd, "@days", SqlDbType.Int, soNgay);
            Add(cmd, "@id", SqlDbType.Int, id);

            if (await cmd.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException(
                    "Không gia hạn được quyền này.");
        }

        // THU HOI QUYEN DOC
        public async Task ThuHoi(int id)
        {
            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                UPDATE dbo.LuotDocSachDienTu
                SET TrangThai = N'ThuHoi'
                WHERE Id = @id
                  AND TrangThai = N'HieuLuc'";

            await using var cmd = new SqlCommand(sql, conn);
            Add(cmd, "@id", SqlDbType.Int, id);

            if (await cmd.ExecuteNonQueryAsync() == 0)
                throw new InvalidOperationException(
                    "Không tìm thấy quyền đang hiệu lực.");
        }

        // NHAT KY TRUY CAP
        public async Task<List<NhatKyVm>> LayNhatKy()
        {
            var result = new List<NhatKyVm>();

            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT TOP (200)
                    n.Id, d.HoTen, s.TieuDe,
                    n.HanhDong, n.ThoiDiem, n.DiaChiIP
                FROM dbo.NhatKyTruyCap n
                JOIN dbo.LuotDocSachDienTu l
                    ON l.Id = n.LuotDocId
                JOIN dbo.DocGia d ON d.Id = l.DocGiaId
                JOIN dbo.TepSachDienTu t
                    ON t.Id = l.TepSachId
                JOIN dbo.Sach s ON s.Id = t.SachId
                ORDER BY n.ThoiDiem DESC";

            await using var cmd = new SqlCommand(sql, conn);
            await using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                result.Add(new NhatKyVm
                {
                    Id = rd.GetInt64(0),
                    DocGia = rd.GetString(1),
                    TenSach = rd.GetString(2),
                    HanhDong = rd.GetString(3),
                    ThoiDiem = rd.GetDateTime(4),
                    DiaChiIP = rd.IsDBNull(5)
                        ? null : rd.GetString(5)
                });
            }

            return result;
        }

        // GOI Y TIM KIEM SACH DIEN TU (AJAX)
        public async Task<List<object>> GoiY(string? term)
        {
            var result = new List<object>();

            if (string.IsNullOrWhiteSpace(term))
                return result;

            term = term.Trim();

            await using var conn = OpenConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT TOP (10) s.TieuDe, s.Isbn
                FROM dbo.Sach s
                WHERE EXISTS (
                    SELECT 1 FROM dbo.TepSachDienTu t
                    WHERE t.SachId = s.Id
                )
                AND (s.TieuDe LIKE N'%' + @term + N'%'
                     OR s.Isbn LIKE N'%' + @term + N'%')
                ORDER BY s.TieuDe";

            await using var cmd = new SqlCommand(sql, conn);
            Add(cmd, "@term", SqlDbType.NVarChar, term, 255);
            await using var rd = await cmd.ExecuteReaderAsync();

            while (await rd.ReadAsync())
            {
                string ten = rd.GetString(0);
                string isbn = rd.IsDBNull(1)
                    ? "Chưa có ISBN"
                    : rd.GetString(1);

                result.Add(new
                {
                    value = ten,
                    label = ten,
                    detail = "ISBN: " + isbn
                });
            }

            return result;
        }

    }
}
