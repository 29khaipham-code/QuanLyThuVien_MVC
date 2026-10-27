/* =====================================================================
   CƠ SỞ DỮ LIỆU QUẢN LÝ THƯ VIỆN (SÁCH GIẤY + SÁCH ĐIỆN TỬ)
   SQL Server 2016+ | 18 bảng
   Nhóm 1: VaiTro, TaiKhoan, LoaiDocGia, DocGia
   Nhóm 2: TheLoai, NhaXuatBan, TacGia, Sach, SachTacGia
   Nhóm 3: BanSaoSach, PhieuMuon, ChiTietMuon
   Nhóm 4: TepSachDienTu, LuotDocSachDienTu, NhatKyTruyCap
   Nhóm 5: DatTruoc, Phat, ThanhLy
   ===================================================================== */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'QuanLyThuVien') IS NULL
    CREATE DATABASE QuanLyThuVien COLLATE Vietnamese_CI_AS;
GO
USE QuanLyThuVien;
GO

/* ===================== NHÓM 1: TÀI KHOẢN VÀ ĐỘC GIẢ ===================== */

CREATE TABLE dbo.VaiTro (
    Id          INT IDENTITY(1,1) NOT NULL,
    TenVaiTro   NVARCHAR(50)  NOT NULL,
    MoTa        NVARCHAR(255) NULL,
    CONSTRAINT PK_VaiTro PRIMARY KEY (Id),
    CONSTRAINT UQ_VaiTro_TenVaiTro UNIQUE (TenVaiTro)
);
GO

CREATE TABLE dbo.TaiKhoan (
    Id            INT IDENTITY(1,1) NOT NULL,
    TenDangNhap   VARCHAR(50)   NOT NULL,
    MatKhauHash   VARCHAR(255)  NOT NULL,
    HoTen         NVARCHAR(100) NOT NULL,
    Email         VARCHAR(100)  NOT NULL,
    SoDienThoai   VARCHAR(20)   NULL,
    HoatDong      BIT           NOT NULL CONSTRAINT DF_TaiKhoan_HoatDong DEFAULT (1),
    NgayTao       DATETIME2     NOT NULL CONSTRAINT DF_TaiKhoan_NgayTao DEFAULT (SYSDATETIME()),
    VaiTroId      INT           NOT NULL,
    CONSTRAINT PK_TaiKhoan PRIMARY KEY (Id),
    CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE (TenDangNhap),
    CONSTRAINT UQ_TaiKhoan_Email UNIQUE (Email),
    CONSTRAINT FK_TaiKhoan_VaiTro FOREIGN KEY (VaiTroId) REFERENCES dbo.VaiTro (Id)
);
CREATE INDEX IX_TaiKhoan_VaiTroId ON dbo.TaiKhoan (VaiTroId);
GO

CREATE TABLE dbo.LoaiDocGia (
    Id               INT IDENTITY(1,1) NOT NULL,
    TenLoai          NVARCHAR(100) NOT NULL,
    SoSachToiDa      INT           NOT NULL,
    SoNgayMuonToiDa  INT           NOT NULL,
    MoTa             NVARCHAR(255) NULL,
    CONSTRAINT PK_LoaiDocGia PRIMARY KEY (Id),
    CONSTRAINT UQ_LoaiDocGia_TenLoai UNIQUE (TenLoai),
    CONSTRAINT CK_LoaiDocGia_SoSach CHECK (SoSachToiDa > 0),
    CONSTRAINT CK_LoaiDocGia_SoNgay CHECK (SoNgayMuonToiDa > 0)
);
GO

CREATE TABLE dbo.DocGia (
    Id             INT IDENTITY(1,1) NOT NULL,
    MaDocGia       VARCHAR(20)   NOT NULL,
    HoTen          NVARCHAR(100) NOT NULL,
    NgaySinh       DATE          NULL,
    GioiTinh       NVARCHAR(10)  NULL,
    Email          VARCHAR(100)  NULL,
    SoDienThoai    VARCHAR(20)   NULL,
    DiaChi         NVARCHAR(255) NULL,
    NgayDangKy     DATE          NOT NULL CONSTRAINT DF_DocGia_NgayDangKy DEFAULT (CAST(SYSDATETIME() AS DATE)),
    NgayHetHan     DATE          NOT NULL,
    TrangThai      NVARCHAR(30)  NOT NULL CONSTRAINT DF_DocGia_TrangThai DEFAULT (N'HoatDong'),
    TaiKhoanId     INT           NULL,   -- tài khoản đăng nhập (bắt buộc nếu muốn đọc sách điện tử)
    LoaiDocGiaId   INT           NOT NULL,
    CONSTRAINT PK_DocGia PRIMARY KEY (Id),
    CONSTRAINT UQ_DocGia_MaDocGia UNIQUE (MaDocGia),
    CONSTRAINT FK_DocGia_TaiKhoan   FOREIGN KEY (TaiKhoanId)   REFERENCES dbo.TaiKhoan (Id),
    CONSTRAINT FK_DocGia_LoaiDocGia FOREIGN KEY (LoaiDocGiaId) REFERENCES dbo.LoaiDocGia (Id),
    CONSTRAINT CK_DocGia_TrangThai CHECK (TrangThai IN (N'HoatDong', N'TamKhoa', N'HetHan')),
    CONSTRAINT CK_DocGia_Han CHECK (NgayHetHan >= NgayDangKy)
);
-- Một tài khoản chỉ gắn với tối đa một độc giả (quan hệ 1 – 0..1)
CREATE UNIQUE INDEX UX_DocGia_TaiKhoanId ON dbo.DocGia (TaiKhoanId) WHERE TaiKhoanId IS NOT NULL;
CREATE INDEX IX_DocGia_LoaiDocGiaId ON dbo.DocGia (LoaiDocGiaId);
GO

/* ===================== NHÓM 2: DANH MỤC SÁCH ===================== */

CREATE TABLE dbo.TheLoai (
    Id           INT IDENTITY(1,1) NOT NULL,
    TenTheLoai   NVARCHAR(100) NOT NULL,
    MoTa         NVARCHAR(255) NULL,
    CONSTRAINT PK_TheLoai PRIMARY KEY (Id),
    CONSTRAINT UQ_TheLoai_Ten UNIQUE (TenTheLoai)
);
GO

CREATE TABLE dbo.NhaXuatBan (
    Id              INT IDENTITY(1,1) NOT NULL,
    TenNhaXuatBan   NVARCHAR(150) NOT NULL,
    DiaChi          NVARCHAR(255) NULL,
    SoDienThoai     VARCHAR(20)   NULL,
    Email           VARCHAR(100)  NULL,
    Website         VARCHAR(255)  NULL,
    CONSTRAINT PK_NhaXuatBan PRIMARY KEY (Id)
);
GO

CREATE TABLE dbo.TacGia (
    Id         INT IDENTITY(1,1) NOT NULL,
    TenTacGia  NVARCHAR(150) NOT NULL,
    TieuSu     NVARCHAR(MAX) NULL,
    NgaySinh   DATE          NULL,
    QuocTich   NVARCHAR(100) NULL,
    CONSTRAINT PK_TacGia PRIMARY KEY (Id)
);
GO

CREATE TABLE dbo.Sach (
    Id            INT IDENTITY(1,1) NOT NULL,
    ISBN          VARCHAR(20)   NULL,
    TieuDe        NVARCHAR(255) NOT NULL,
    TheLoaiId     INT           NOT NULL,
    NhaXuatBanId  INT           NOT NULL,
    NamXuatBan    INT           NULL,
    LanTaiBan     INT           NULL,
    MoTa          NVARCHAR(MAX) NULL,
    AnhBiaUrl     VARCHAR(255)  NULL,
    TrangThai     NVARCHAR(30)  NOT NULL CONSTRAINT DF_Sach_TrangThai DEFAULT (N'HoatDong'),
    NgayTao       DATETIME2     NOT NULL CONSTRAINT DF_Sach_NgayTao DEFAULT (SYSDATETIME()),
    NgayCapNhat   DATETIME2     NULL,
    CONSTRAINT PK_Sach PRIMARY KEY (Id),
    CONSTRAINT FK_Sach_TheLoai    FOREIGN KEY (TheLoaiId)    REFERENCES dbo.TheLoai (Id),
    CONSTRAINT FK_Sach_NhaXuatBan FOREIGN KEY (NhaXuatBanId) REFERENCES dbo.NhaXuatBan (Id),
    CONSTRAINT CK_Sach_TrangThai CHECK (TrangThai IN (N'HoatDong', N'NgungPhucVu')),
    CONSTRAINT CK_Sach_Nam CHECK (NamXuatBan IS NULL OR NamXuatBan > 0),
    CONSTRAINT CK_Sach_LanTaiBan CHECK (LanTaiBan IS NULL OR LanTaiBan > 0)
);
-- ISBN duy nhất khi có giá trị
CREATE UNIQUE INDEX UX_Sach_ISBN ON dbo.Sach (ISBN) WHERE ISBN IS NOT NULL;
CREATE INDEX IX_Sach_TheLoaiId ON dbo.Sach (TheLoaiId);
CREATE INDEX IX_Sach_NhaXuatBanId ON dbo.Sach (NhaXuatBanId);
GO

CREATE TABLE dbo.SachTacGia (
    SachId    INT NOT NULL,
    TacGiaId  INT NOT NULL,
    CONSTRAINT PK_SachTacGia PRIMARY KEY (SachId, TacGiaId),
    CONSTRAINT FK_SachTacGia_Sach   FOREIGN KEY (SachId)   REFERENCES dbo.Sach (Id),
    CONSTRAINT FK_SachTacGia_TacGia FOREIGN KEY (TacGiaId) REFERENCES dbo.TacGia (Id)
);
CREATE INDEX IX_SachTacGia_TacGiaId ON dbo.SachTacGia (TacGiaId);
GO

/* ===================== NHÓM 3: BẢN SAO VẬT LÝ VÀ MƯỢN/TRẢ ===================== */

CREATE TABLE dbo.BanSaoSach (
    Id         INT IDENTITY(1,1) NOT NULL,
    SachId     INT           NOT NULL,
    MaVach     VARCHAR(50)   NOT NULL,
    TrangThai  NVARCHAR(30)  NOT NULL CONSTRAINT DF_BanSao_TrangThai DEFAULT (N'SanSang'),
    TinhTrang  NVARCHAR(30)  NOT NULL CONSTRAINT DF_BanSao_TinhTrang DEFAULT (N'Tot'),
    NgayNhap   DATE          NULL,
    ViTri      NVARCHAR(100) NULL,
    GiaTri     DECIMAL(18,2) NULL,
    CONSTRAINT PK_BanSaoSach PRIMARY KEY (Id),
    CONSTRAINT UQ_BanSaoSach_MaVach UNIQUE (MaVach),
    CONSTRAINT FK_BanSaoSach_Sach FOREIGN KEY (SachId) REFERENCES dbo.Sach (Id),
    CONSTRAINT CK_BanSaoSach_TrangThai CHECK (TrangThai IN
        (N'SanSang', N'DangMuon', N'DaDat', N'HuHong', N'Mat', N'DangSua', N'DaThanhLy')),
    CONSTRAINT CK_BanSaoSach_TinhTrang CHECK (TinhTrang IN (N'Moi', N'Tot', N'Cu', N'HuHong')),
    CONSTRAINT CK_BanSaoSach_GiaTri CHECK (GiaTri IS NULL OR GiaTri >= 0)
);
CREATE INDEX IX_BanSaoSach_SachId_TrangThai ON dbo.BanSaoSach (SachId, TrangThai);
GO

CREATE TABLE dbo.PhieuMuon (
    Id             INT IDENTITY(1,1) NOT NULL,
    MaPhieu        VARCHAR(20)   NOT NULL,
    DocGiaId       INT           NOT NULL,
    NhanVienLapId  INT           NOT NULL,   -- TaiKhoan của nhân viên lập phiếu
    NgayMuon       DATE          NOT NULL CONSTRAINT DF_PhieuMuon_NgayMuon DEFAULT (CAST(SYSDATETIME() AS DATE)),
    TrangThai      NVARCHAR(30)  NOT NULL CONSTRAINT DF_PhieuMuon_TrangThai DEFAULT (N'DangMuon'),
    GhiChu         NVARCHAR(500) NULL,
    NgayTao        DATETIME2     NOT NULL CONSTRAINT DF_PhieuMuon_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_PhieuMuon PRIMARY KEY (Id),
    CONSTRAINT UQ_PhieuMuon_MaPhieu UNIQUE (MaPhieu),
    CONSTRAINT FK_PhieuMuon_DocGia   FOREIGN KEY (DocGiaId)      REFERENCES dbo.DocGia (Id),
    CONSTRAINT FK_PhieuMuon_NhanVien FOREIGN KEY (NhanVienLapId) REFERENCES dbo.TaiKhoan (Id),
    CONSTRAINT CK_PhieuMuon_TrangThai CHECK (TrangThai IN (N'DangMuon', N'DaTraHet', N'Huy'))
);
CREATE INDEX IX_PhieuMuon_DocGiaId ON dbo.PhieuMuon (DocGiaId);
CREATE INDEX IX_PhieuMuon_NhanVienLapId ON dbo.PhieuMuon (NhanVienLapId);
GO

CREATE TABLE dbo.ChiTietMuon (
    Id              INT IDENTITY(1,1) NOT NULL,
    PhieuMuonId     INT           NOT NULL,
    BanSaoId        INT           NOT NULL,
    HanTra          DATE          NOT NULL,
    NgayTra         DATETIME2     NULL,       -- NULL = chưa trả
    TinhTrangTruoc  NVARCHAR(255) NOT NULL,
    TinhTrangSau    NVARCHAR(255) NULL,
    GhiChu          NVARCHAR(500) NULL,
    CONSTRAINT PK_ChiTietMuon PRIMARY KEY (Id),
    CONSTRAINT UQ_ChiTietMuon_Phieu_BanSao UNIQUE (PhieuMuonId, BanSaoId),
    CONSTRAINT FK_ChiTietMuon_PhieuMuon FOREIGN KEY (PhieuMuonId) REFERENCES dbo.PhieuMuon (Id),
    CONSTRAINT FK_ChiTietMuon_BanSao    FOREIGN KEY (BanSaoId)    REFERENCES dbo.BanSaoSach (Id),
    CONSTRAINT CK_ChiTietMuon_TinhTrangSau CHECK (NgayTra IS NOT NULL OR TinhTrangSau IS NULL)
);
-- Một bản sao chỉ có MỘT lượt mượn chưa trả (NgayTra IS NULL) tại một thời điểm.
-- Index 2 cột (không lọc) để EF Core scaffold quan hệ BanSaoSach - ChiTietMuon là 1-N;
-- SQL Server coi các giá trị NULL là bằng nhau trong unique index nên không thể có 2 dòng NgayTra NULL.
-- Index này cũng đã phục vụ tra cứu theo BanSaoId (cột đứng đầu).
CREATE UNIQUE INDEX UX_ChiTietMuon_BanSao_NgayTra ON dbo.ChiTietMuon (BanSaoId, NgayTra);
GO

/* ===================== NHÓM 4: SÁCH ĐIỆN TỬ ===================== */

CREATE TABLE dbo.TepSachDienTu (
    Id               INT IDENTITY(1,1) NOT NULL,
    SachId           INT           NOT NULL,
    DuongDanTep      NVARCHAR(500) NOT NULL,   -- đường dẫn nội bộ/kho lưu trữ, không công khai
    DinhDang         VARCHAR(10)   NOT NULL,   -- PDF, EPUB, ...
    DungLuong        BIGINT        NULL,       -- byte
    CheDoTruyCap     NVARCHAR(30)  NOT NULL CONSTRAINT DF_TepSach_CheDo DEFAULT (N'DocTrucTuyen'),
    TrangThai        NVARCHAR(30)  NOT NULL CONSTRAINT DF_TepSach_TrangThai DEFAULT (N'HoatDong'),
    NgayTaiLen       DATETIME2     NOT NULL CONSTRAINT DF_TepSach_NgayTaiLen DEFAULT (SYSDATETIME()),
    NgayHetBanQuyen  DATE          NULL,
    CONSTRAINT PK_TepSachDienTu PRIMARY KEY (Id),
    CONSTRAINT FK_TepSach_Sach FOREIGN KEY (SachId) REFERENCES dbo.Sach (Id),
    CONSTRAINT CK_TepSach_CheDo CHECK (CheDoTruyCap IN (N'DocTrucTuyen', N'ChoPhepTai')),
    CONSTRAINT CK_TepSach_TrangThai CHECK (TrangThai IN (N'HoatDong', N'TamKhoa', N'HetBanQuyen')),
    CONSTRAINT CK_TepSach_DungLuong CHECK (DungLuong IS NULL OR DungLuong >= 0)
);
CREATE INDEX IX_TepSach_SachId ON dbo.TepSachDienTu (SachId);
GO

CREATE TABLE dbo.LuotDocSachDienTu (
    Id              INT IDENTITY(1,1) NOT NULL,
    DocGiaId        INT          NOT NULL,
    TepSachId       INT          NOT NULL,
    NgayBatDau      DATETIME2    NOT NULL CONSTRAINT DF_LuotDoc_NgayBatDau DEFAULT (SYSDATETIME()),
    NgayHetHan      DATETIME2    NOT NULL,
    TrangThai       NVARCHAR(30) NOT NULL CONSTRAINT DF_LuotDoc_TrangThai DEFAULT (N'HieuLuc'),
    LanTruyCapCuoi  DATETIME2    NULL,
    CONSTRAINT PK_LuotDocSachDienTu PRIMARY KEY (Id),
    CONSTRAINT FK_LuotDoc_DocGia  FOREIGN KEY (DocGiaId)  REFERENCES dbo.DocGia (Id),
    CONSTRAINT FK_LuotDoc_TepSach FOREIGN KEY (TepSachId) REFERENCES dbo.TepSachDienTu (Id),
    CONSTRAINT CK_LuotDoc_TrangThai CHECK (TrangThai IN (N'HieuLuc', N'HetHan', N'ThuHoi')),
    CONSTRAINT CK_LuotDoc_Han CHECK (NgayHetHan > NgayBatDau)
);
-- Mỗi độc giả chỉ có một quyền đọc đang hiệu lực cho một tệp
CREATE UNIQUE INDEX UX_LuotDoc_HieuLuc ON dbo.LuotDocSachDienTu (DocGiaId, TepSachId) WHERE TrangThai = N'HieuLuc';
CREATE INDEX IX_LuotDoc_TepSachId ON dbo.LuotDocSachDienTu (TepSachId);
GO

CREATE TABLE dbo.NhatKyTruyCap (
    Id         BIGINT IDENTITY(1,1) NOT NULL,
    LuotDocId  INT          NOT NULL,
    ThoiDiem   DATETIME2    NOT NULL CONSTRAINT DF_NhatKy_ThoiDiem DEFAULT (SYSDATETIME()),
    HanhDong   NVARCHAR(30) NOT NULL,
    DiaChiIP   VARCHAR(45)  NULL,
    CONSTRAINT PK_NhatKyTruyCap PRIMARY KEY (Id),
    CONSTRAINT FK_NhatKy_LuotDoc FOREIGN KEY (LuotDocId) REFERENCES dbo.LuotDocSachDienTu (Id),
    CONSTRAINT CK_NhatKy_HanhDong CHECK (HanhDong IN (N'Mo', N'Doc', N'Tai'))
);
CREATE INDEX IX_NhatKy_LuotDocId_ThoiDiem ON dbo.NhatKyTruyCap (LuotDocId, ThoiDiem);
GO

/* ===================== NHÓM 5: ĐẶT TRƯỚC, TIỀN PHẠT, THANH LÝ ===================== */

CREATE TABLE dbo.DatTruoc (
    Id           INT IDENTITY(1,1) NOT NULL,
    DocGiaId     INT          NOT NULL,
    SachId       INT          NOT NULL,
    NgayDat      DATETIME2    NOT NULL CONSTRAINT DF_DatTruoc_NgayDat DEFAULT (SYSDATETIME()),
    NgayHetHan   DATETIME2    NULL,        -- hạn giữ chỗ, đặt khi có bản sao sẵn sàng
    BanSaoGiuId  INT          NULL,        -- bản sao đang được giữ cho độc giả
    TrangThai    NVARCHAR(30) NOT NULL CONSTRAINT DF_DatTruoc_TrangThai DEFAULT (N'DangCho'),
    CONSTRAINT PK_DatTruoc PRIMARY KEY (Id),
    CONSTRAINT FK_DatTruoc_DocGia  FOREIGN KEY (DocGiaId)    REFERENCES dbo.DocGia (Id),
    CONSTRAINT FK_DatTruoc_Sach    FOREIGN KEY (SachId)      REFERENCES dbo.Sach (Id),
    CONSTRAINT FK_DatTruoc_BanSao  FOREIGN KEY (BanSaoGiuId) REFERENCES dbo.BanSaoSach (Id),
    CONSTRAINT CK_DatTruoc_TrangThai CHECK (TrangThai IN (N'DangCho', N'DaGiu', N'DaNhan', N'QuaHan', N'Huy')),
    CONSTRAINT CK_DatTruoc_Han CHECK (NgayHetHan IS NULL OR NgayHetHan >= NgayDat),
    CONSTRAINT CK_DatTruoc_DaGiu CHECK (TrangThai <> N'DaGiu' OR BanSaoGiuId IS NOT NULL)
);
-- Một độc giả không đặt trùng một đầu sách khi yêu cầu còn hiệu lực
CREATE UNIQUE INDEX UX_DatTruoc_ConHieuLuc ON dbo.DatTruoc (DocGiaId, SachId)
    WHERE TrangThai IN (N'DangCho', N'DaGiu');   -- filtered index không cho dùng OR, chỉ dùng IN
-- Hàng đợi theo đầu sách, sắp theo thời điểm đặt
CREATE INDEX IX_DatTruoc_Sach_HangDoi ON dbo.DatTruoc (SachId, NgayDat);
CREATE INDEX IX_DatTruoc_BanSaoGiuId ON dbo.DatTruoc (BanSaoGiuId);
GO

CREATE TABLE dbo.Phat (
    Id             INT IDENTITY(1,1) NOT NULL,
    ChiTietMuonId  INT           NOT NULL,
    LoaiPhat       NVARCHAR(50)  NOT NULL,
    LyDo           NVARCHAR(500) NULL,
    SoTien         DECIMAL(18,2) NOT NULL,
    DaThanhToan    BIT           NOT NULL CONSTRAINT DF_Phat_DaThanhToan DEFAULT (0),
    NgayThanhToan  DATE          NULL,
    NgayTao        DATETIME2     NOT NULL CONSTRAINT DF_Phat_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Phat PRIMARY KEY (Id),
    CONSTRAINT FK_Phat_ChiTietMuon FOREIGN KEY (ChiTietMuonId) REFERENCES dbo.ChiTietMuon (Id),
    CONSTRAINT CK_Phat_Loai CHECK (LoaiPhat IN (N'TraMuon', N'HuHong', N'MatSach')),
    CONSTRAINT CK_Phat_SoTien CHECK (SoTien >= 0),
    CONSTRAINT CK_Phat_ThanhToan CHECK (
        (DaThanhToan = 1 AND NgayThanhToan IS NOT NULL) OR
        (DaThanhToan = 0 AND NgayThanhToan IS NULL))
);
-- Mỗi lượt mượn có thể có nhiều khoản phạt (trả muộn, hư hỏng, mất) nhưng mỗi loại tối đa một khoản.
-- Index 2 cột để EF Core scaffold quan hệ ChiTietMuon - Phat là 1-N; ChiTietMuonId đứng đầu nên vẫn phục vụ tra cứu.
CREATE UNIQUE INDEX UX_Phat_ChiTiet_Loai ON dbo.Phat (ChiTietMuonId, LoaiPhat);
GO

CREATE TABLE dbo.ThanhLy (
    Id           INT IDENTITY(1,1) NOT NULL,
    BanSaoId     INT           NOT NULL,
    LyDo         NVARCHAR(500) NOT NULL,
    NgayThanhLy  DATE          NOT NULL CONSTRAINT DF_ThanhLy_Ngay DEFAULT (CAST(SYSDATETIME() AS DATE)),
    NhanVienId   INT           NOT NULL,   -- TaiKhoan thực hiện thanh lý
    CONSTRAINT PK_ThanhLy PRIMARY KEY (Id),
    CONSTRAINT UQ_ThanhLy_BanSaoId UNIQUE (BanSaoId),     -- mỗi cuốn thanh lý một lần
    CONSTRAINT FK_ThanhLy_BanSao  FOREIGN KEY (BanSaoId)   REFERENCES dbo.BanSaoSach (Id),
    CONSTRAINT FK_ThanhLy_NhanVien FOREIGN KEY (NhanVienId) REFERENCES dbo.TaiKhoan (Id)
);
CREATE INDEX IX_ThanhLy_NhanVienId ON dbo.ThanhLy (NhanVienId);
GO

/* ===================== DỮ LIỆU KHỞI TẠO TỐI THIỂU ===================== */

INSERT INTO dbo.VaiTro (TenVaiTro, MoTa) VALUES
 (N'QuanTri', N'Quản trị hệ thống'),
 (N'ThuThu',  N'Thủ thư: mượn/trả, quản lý sách'),
 (N'DocGia',  N'Độc giả đăng nhập để đọc sách điện tử và đặt trước');
GO

/* ===================== VIEW THỐNG KÊ SỐ LƯỢNG SÁCH ===================== */

CREATE VIEW dbo.vw_SoLuongSach AS
SELECT  s.Id AS SachId,
        s.TieuDe,
        COUNT(b.Id)                                            AS TongBanSao,
        SUM(CASE WHEN b.TrangThai = N'SanSang'   THEN 1 ELSE 0 END) AS SanSang,
        SUM(CASE WHEN b.TrangThai = N'DangMuon'  THEN 1 ELSE 0 END) AS DangMuon,
        SUM(CASE WHEN b.TrangThai = N'DaDat'     THEN 1 ELSE 0 END) AS DaDat,
        SUM(CASE WHEN b.TrangThai = N'HuHong'    THEN 1 ELSE 0 END) AS HuHong,
        SUM(CASE WHEN b.TrangThai = N'Mat'       THEN 1 ELSE 0 END) AS Mat,
        SUM(CASE WHEN b.TrangThai = N'DangSua'   THEN 1 ELSE 0 END) AS DangSua,
        SUM(CASE WHEN b.TrangThai = N'DaThanhLy' THEN 1 ELSE 0 END) AS DaThanhLy
FROM dbo.Sach s
LEFT JOIN dbo.BanSaoSach b ON b.SachId = s.Id
GROUP BY s.Id, s.TieuDe;
GO
