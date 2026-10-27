USE QuanLyThuVien;
GO

-- Kiểm tra thể loại
SELECT * FROM dbo.TheLoai;

-- Kiểm tra nhà xuất bản
SELECT * FROM dbo.NhaXuatBan;

-- Kiểm tra tác giả
SELECT * FROM dbo.TacGia;

-- Kiểm tra sách và ảnh bìa
SELECT Id, TieuDe, TheLoaiId, AnhBiaUrl, TrangThai
FROM dbo.Sach;

SELECT Id, TieuDe, TheLoaiId, AnhBiaUrl, TrangThai
FROM dbo.Sach;

SELECT Id, TenTheLoai
FROM dbo.TheLoai;

EXEC sp_help 'dbo.Sach';
EXEC sp_help 'dbo.NhaXuatBan';
EXEC sp_help 'dbo.TacGia';
EXEC sp_help 'dbo.SachTacGia';

EXEC sp_help 'dbo.TheLoai';
EXEC sp_help 'dbo.NhaXuatBan';
EXEC sp_help 'dbo.TacGia';

SELECT name
FROM sys.tables
ORDER BY name;



USE QuanLyThuVien;
GO

SELECT * FROM dbo.TheLoai;
SELECT * FROM dbo.NhaXuatBan;
SELECT * FROM dbo.TacGia;

EXEC sp_help 'dbo.NhaXuatBan';
EXEC sp_help 'dbo.TacGia';
EXEC sp_help 'dbo.SachTacGia';
