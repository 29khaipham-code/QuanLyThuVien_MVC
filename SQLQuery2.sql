
USE QuanLyThuVien;
GO

-- 1. Thêm thể loại
INSERT INTO dbo.TheLoai (TenTheLoai, MoTa)
VALUES
(N'Tiểu thuyết', N'Tác phẩm văn học hư cấu'),
(N'Kỹ năng sống', N'Sách phát triển bản thân'),
(N'Khoa học', N'Sách khoa học và khám phá'),
(N'Lịch sử', N'Sách về lịch sử'),
(N'Truyện thiếu nhi', N'Sách dành cho thiếu nhi'),
(N'Công nghệ', N'Sách về công nghệ thông tin'),
(N'Kinh tế', N'Sách kinh tế và quản trị'),
(N'Tâm lý học', N'Sách về tâm lý con người');

-- 2. Thêm nhà xuất bản
INSERT INTO dbo.NhaXuatBan
    (TenNhaXuatBan, DiaChi, SoDienThoai, Email, Website)
VALUES
(N'Nhà xuất bản Trẻ', N'TP. Hồ Chí Minh', NULL, NULL, NULL),
(N'Nhà xuất bản Kim Đồng', N'Hà Nội', NULL, NULL, NULL),
(N'Nhà xuất bản Giáo dục Việt Nam', N'Hà Nội', NULL, NULL, NULL),
(N'Nhà xuất bản Lao động', N'Hà Nội', NULL, NULL, NULL),
(N'Nhà xuất bản Thế Giới', N'Hà Nội', NULL, NULL, NULL);

-- 3. Kiểm tra kết quả
SELECT * FROM dbo.TheLoai;
SELECT * FROM dbo.NhaXuatBan;




-- Thêm tác giả mẫu
INSERT INTO dbo.TacGia (TenTacGia, TieuSu, NgaySinh, QuocTich)
VALUES
(N'Nguyễn Nhật Ánh', N'Nhà văn Việt Nam, nổi tiếng với các tác phẩm viết cho tuổi mới lớn.', NULL, N'Việt Nam'),
(N'Tô Hoài', N'Nhà văn Việt Nam, có nhiều tác phẩm dành cho thiếu nhi.', NULL, N'Việt Nam'),
(N'Nam Cao', N'Nhà văn hiện thực Việt Nam.', NULL, N'Việt Nam'),
(N'Dale Carnegie', N'Tác giả sách phát triển bản thân.', NULL, N'Mỹ'),
(N'Nguyễn Hiến Lê', N'Tác giả, dịch giả và học giả Việt Nam.', NULL, N'Việt Nam'),
(N'Yuval Noah Harari', N'Tác giả sách lịch sử và khoa học xã hội.', NULL, N'Israel'),
(N'Nguyễn Duy Cần', N'Tác giả sách học thuật và triết học.', NULL, N'Việt Nam'),
(N'J. K. Rowling', N'Tác giả bộ truyện Harry Potter.', NULL, N'Anh');

-- Kiểm tra tác giả
SELECT * FROM dbo.TacGia;

-- Kiểm tra cấu trúc bảng liên kết sách và tác giả
EXEC sp_help 'dbo.SachTacGia';




INSERT INTO dbo.Sach
    (ISBN, TieuDe, TheLoaiId, NhaXuatBanId,
     NamXuatBan, LanTaiBan, MoTa)
VALUES
('9780000000001', N'Mắt biếc', 1, 1, 2019, 1, N'Câu chuyện tình yêu tuổi học trò và những ký ức khó quên.'),
('9780000000002', N'Tôi thấy hoa vàng trên cỏ xanh', 1, 1, 2018, 1, N'Tuổi thơ và tình cảm gia đình qua góc nhìn trẻ nhỏ.'),
('9780000000003', N'Cho tôi xin một vé đi tuổi thơ', 1, 1, 2017, 1, N'Hồi ức tuổi thơ hồn nhiên và những suy ngẫm về cuộc sống.'),
('9780000000004', N'Dế Mèn phiêu lưu ký', 5, 2, 2019, 1, N'Cuộc phiêu lưu của chú Dế Mèn qua nhiều vùng đất.'),
('9780000000005', N'Đắc nhân tâm', 2, 4, 2020, 1, N'Những nguyên tắc giao tiếp và xây dựng mối quan hệ.'),
('9780000000006', N'Quẳng gánh lo đi và vui sống', 2, 4, 2019, 1, N'Những gợi ý để quản lý lo âu trong cuộc sống.'),
('9780000000007', N'Nhà giả kim', 1, 5, 2020, 1, N'Hành trình theo đuổi ước mơ và khám phá bản thân.'),
('9780000000008', N'Lược sử loài người', 3, 5, 2021, 1, N'Khái quát lịch sử phát triển của loài người.'),
('9780000000009', N'Harry Potter và Hòn đá Phù thủy', 5, 2, 2018, 1, N'Cuộc phiêu lưu đầu tiên của Harry Potter tại trường phép thuật.'),
('9780000000010', N'Không gia đình', 5, 2, 2017, 1, N'Câu chuyện về hành trình trưởng thành và tình người.'),
('9780000000011', N'Số đỏ', 1, 1, 2016, 1, N'Tác phẩm trào phúng về xã hội Việt Nam xưa.'),
('9780000000012', N'Chí Phèo', 1, 4, 2016, 1, N'Tác phẩm văn học hiện thực về số phận con người.'),
('9780000000013', N'Tư duy nhanh và chậm', 8, 5, 2020, 1, N'Khám phá cách con người suy nghĩ và đưa ra quyết định.'),
('9780000000014', N'Đời thay đổi khi chúng ta thay đổi', 2, 4, 2018, 1, N'Những góc nhìn về thay đổi bản thân và cuộc sống.'),
('9780000000015', N'Khuyến học', 2, 3, 2019, 1, N'Bàn về vai trò của học tập đối với mỗi cá nhân và xã hội.'),
('9780000000016', N'Lịch sử Việt Nam', 4, 3, 2020, 1, N'Tổng quan về các giai đoạn lịch sử Việt Nam.'),
('9780000000017', N'Nhập môn trí tuệ nhân tạo', 6, 3, 2022, 1, N'Giới thiệu các khái niệm cơ bản về trí tuệ nhân tạo.'),
('9780000000018', N'Kinh tế học cơ bản', 7, 4, 2021, 1, N'Giới thiệu những khái niệm kinh tế học nền tảng.'),
('9780000000019', N'Tâm lý học đám đông', 8, 5, 2019, 1, N'Tìm hiểu hành vi và tâm lý của con người trong đám đông.'),
('9780000000020', N'Học cách học', 2, 3, 2021, 1, N'Các phương pháp tổ chức việc học và ghi nhớ kiến thức.');

SELECT Id, ISBN, TieuDe, TheLoaiId, NhaXuatBanId
FROM dbo.Sach
ORDER BY Id;


SELECT Id, TieuDe
FROM dbo.Sach
ORDER BY Id;

SELECT Id, TenTacGia
FROM dbo.TacGia
ORDER BY Id;


SELECT Id, TieuDe
FROM dbo.Sach
ORDER BY Id;

SELECT Id, TenTacGia
FROM dbo.TacGia
ORDER BY Id;

SELECT * FROM dbo.SachTacGia;


INSERT INTO dbo.SachTacGia (SachId, TacGiaId)
SELECT s.Id, tg.Id
FROM (VALUES
    (N'Mắt biếc', N'Nguyễn Nhật Ánh'),
    (N'Tôi thấy hoa vàng trên cỏ xanh', N'Nguyễn Nhật Ánh'),
    (N'Cho tôi xin một vé đi tuổi thơ', N'Nguyễn Nhật Ánh'),
    (N'Dế Mèn phiêu lưu ký', N'Tô Hoài'),
    (N'Đắc nhân tâm', N'Dale Carnegie'),
    (N'Quẳng gánh lo đi và vui sống', N'Dale Carnegie'),
    (N'Nhà giả kim', N'Nguyễn Hiến Lê'),
    (N'Lược sử loài người', N'Yuval Noah Harari'),
    (N'Harry Potter và Hòn đá Phù thủy', N'J. K. Rowling'),
    (N'Không gia đình', N'Tô Hoài'),
    (N'Số đỏ', N'Nam Cao'),
    (N'Chí Phèo', N'Nam Cao'),
    (N'Tư duy nhanh và chậm', N'Nguyễn Duy Cần'),
    (N'Đời thay đổi khi chúng ta thay đổi', N'Dale Carnegie'),
    (N'Khuyến học', N'Nguyễn Hiến Lê'),
    (N'Lịch sử Việt Nam', N'Nguyễn Hiến Lê'),
    (N'Nhập môn trí tuệ nhân tạo', N'Nguyễn Duy Cần'),
    (N'Kinh tế học cơ bản', N'Nguyễn Hiến Lê'),
    (N'Tâm lý học đám đông', N'Nguyễn Duy Cần'),
    (N'Học cách học', N'Nguyễn Hiến Lê')
) AS ds(TieuDe, TenTacGia)
JOIN dbo.Sach s ON s.TieuDe = ds.TieuDe
JOIN dbo.TacGia tg ON tg.TenTacGia = ds.TenTacGia
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.SachTacGia stg
    WHERE stg.SachId = s.Id
      AND stg.TacGiaId = tg.Id
);

-- Kiểm tra kết quả
SELECT s.TieuDe, tg.TenTacGia
FROM dbo.SachTacGia stg
JOIN dbo.Sach s ON s.Id = stg.SachId
JOIN dbo.TacGia tg ON tg.Id = stg.TacGiaId
ORDER BY s.Id;


SELECT Id, TieuDe, AnhBiaUrl, TrangThai
FROM dbo.Sach
ORDER BY Id;








UPDATE dbo.Sach
SET AnhBiaUrl = CASE TieuDe
    WHEN N'Mắt biếc'
        THEN '/images/sach/mat-biec.jpg'
    WHEN N'Tôi thấy hoa vàng trên cỏ xanh'
        THEN '/images/sach/hoa-vang-tren-co-xanh.jpg'
    WHEN N'Cho tôi xin một vé đi tuổi thơ'
        THEN '/images/sach/ve-di-tuoi-tho.jpg'
    WHEN N'Dế Mèn phiêu lưu ký'
        THEN '/images/sach/de-men-phieu-luu-ky.jpg'
    WHEN N'Đắc nhân tâm'
        THEN '/images/sach/dac-nhan-tam.jpg'
END
WHERE TieuDe IN (
    N'Mắt biếc',
    N'Tôi thấy hoa vàng trên cỏ xanh',
    N'Cho tôi xin một vé đi tuổi thơ',
    N'Dế Mèn phiêu lưu ký',
    N'Đắc nhân tâm'
);

SELECT TieuDe, AnhBiaUrl
FROM dbo.Sach
WHERE AnhBiaUrl IS NOT NULL;

SELECT Id, TieuDe, AnhBiaUrl
FROM Sach;



UPDATE dbo.Sach
SET AnhBiaUrl = '/images/sach/ve-di-tuoi-tho.jpg'
WHERE TieuDe = N'Cho tôi xin một vé đi tuổi thơ';


SELECT Id, TieuDe, AnhBiaUrl
FROM dbo.Sach
WHERE AnhBiaUrl IS NULL
ORDER BY Id;

UPDATE dbo.Sach
SET AnhBiaUrl = CASE Id
    WHEN 6  THEN '/images/sach/quang-ganh-lo-di-va-vui-song.jpg'
    WHEN 7  THEN '/images/sach/nha-gia-kim.jpg'
    WHEN 8  THEN '/images/sach/luoc-su-loai-nguoi.jpg'
    WHEN 9  THEN '/images/sach/harry-potter-hon-da-phu-thuy.jpg'
    WHEN 10 THEN '/images/sach/khong-gia-dinh.jpg'
    WHEN 11 THEN '/images/sach/so-do.jpg'
    WHEN 12 THEN '/images/sach/chi-pheo.jpg'
    WHEN 13 THEN '/images/sach/tu-duy-nhanh-va-cham.jpg'
    WHEN 14 THEN '/images/sach/doi-thay-doi-khi-chung-ta-thay-doi.jpg'
    WHEN 15 THEN '/images/sach/khuyen-hoc.jpg'
    WHEN 16 THEN '/images/sach/lich-su-viet-nam.jpg'
    WHEN 17 THEN '/images/sach/nhap-mon-tri-tue-nhan-tao.jpg'
    WHEN 18 THEN '/images/sach/kinh-te-hoc-co-ban.jpg'
    WHEN 19 THEN '/images/sach/tam-ly-hoc-dam-dong.jpg'
    WHEN 20 THEN '/images/sach/hoc-cach-hoc.jpg'
END
WHERE Id BETWEEN 6 AND 20;