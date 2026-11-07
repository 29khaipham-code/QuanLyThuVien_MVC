
using Microsoft.Data.SqlClient;
using QuanLyThuVien.ViewModels.Admin;
using QuanLyThuVien.ViewModels.Admin.TongQuan;

namespace QuanLyThuVien.Services
{
    public class DashboardThongKeService
    {
        private readonly string _connectionString;

        public DashboardThongKeService(
            IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString(
                    "QuanLyThuVienConnectionString")
                ?? throw new InvalidOperationException(
                    "Không tìm thấy chuỗi kết nối CSDL.");
        }

        public async Task<DashboardThongKeViewModel>
            LayThongKeAsync()
        {
            var model = new DashboardThongKeViewModel();

            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM dbo.Sach)
                        AS TongDauSach,

                    (SELECT COUNT(*) FROM dbo.DocGia)
                        AS TongDocGia,

                    (SELECT COUNT(*)
                     FROM dbo.ChiTietMuon
                     WHERE NgayTra IS NULL)
                        AS SachDangMuon,

                    (SELECT COUNT(*)
                     FROM dbo.ChiTietMuon
                     WHERE NgayTra IS NULL
                       AND HanTra < CAST(
                           SYSDATETIME() AS DATE))
                        AS MuonQuaHan,

                    (SELECT COUNT(*)
                     FROM dbo.BanSaoSach)
                        AS TongBanSao,

                    (SELECT COUNT(*)
                     FROM dbo.BanSaoSach
                     WHERE TrangThai = N'SanSang')
                        AS BanSaoSanSang,

                    (SELECT COUNT(*)
                     FROM dbo.TepSachDienTu
                     WHERE TrangThai = N'HoatDong'
                       AND (
                           NgayHetBanQuyen IS NULL
                           OR NgayHetBanQuyen >=
                               CAST(SYSDATETIME() AS DATE)
                       ))
                        AS TongSachDienTu,

                    (SELECT COUNT(*)
                     FROM dbo.DatTruoc
                     WHERE TrangThai = N'DangCho')
                        AS DatTruocDangCho,

                    (SELECT COUNT(*)
                     FROM dbo.Phat
                     WHERE DaThanhToan = 0)
                        AS PhatChuaThanhToan
            ";

            await using var conn =
                new SqlConnection(_connectionString);

            await conn.OpenAsync();

            await using var cmd =
                new SqlCommand(sql, conn);

            await using var reader =
                await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                model.TongDauSach = reader.GetInt32(0);
                model.TongDocGia = reader.GetInt32(1);
                model.SachDangMuon = reader.GetInt32(2);
                model.MuonQuaHan = reader.GetInt32(3);
                model.TongBanSao = reader.GetInt32(4);
                model.BanSaoSanSang = reader.GetInt32(5);
                model.TongSachDienTu = reader.GetInt32(6);
                model.DatTruocDangCho = reader.GetInt32(7);
                model.PhatChuaThanhToan = reader.GetInt32(8);
            }

            return model;
        }
    }
}
