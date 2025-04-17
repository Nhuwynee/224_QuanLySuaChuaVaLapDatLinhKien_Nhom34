CREATE DATABASE QuanLySuaChuaVaLapDat
GO
USE QuanLySuaChuaVaLapDat
GO

CREATE TABLE ThanhPho (
    idThanhPho CHAR(7) PRIMARY KEY,
    tenThanhPho NVARCHAR(150) NOT NULL UNIQUE
);

CREATE TABLE Quan (
    idQuan CHAR(7) PRIMARY KEY,
    idThanhPho CHAR(7) FOREIGN KEY REFERENCES ThanhPho(idThanhPho),
    tenQuan NVARCHAR(150) NOT NULL UNIQUE
);

CREATE TABLE Phuong (
    idPhuong CHAR(7) PRIMARY KEY,
    idQuan CHAR(7) FOREIGN KEY REFERENCES Quan(idQuan),
    idThanhPho CHAR(7) FOREIGN KEY REFERENCES ThanhPho(idThanhPho),
    tenPhuong NVARCHAR(150) NOT NULL UNIQUE
);


CREATE TABLE [Role] (
    idRole CHAR(7) PRIMARY KEY,
    tenRole NVARCHAR(150) NOT NULL UNIQUE
);

-- KH001
-- NVKT01
CREATE TABLE [User] (
    idUser CHAR(7) PRIMARY KEY,
    idRole CHAR(7) FOREIGN KEY REFERENCES Role(idRole),
    idPhuong CHAR(7) FOREIGN KEY REFERENCES Phuong(idPhuong),
    tenUser VARCHAR(50) NOT NULL UNIQUE,
    hoVaTen NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15) NOT NULL UNIQUE,
    matKhau VARCHAR(100) NOT NULL,
    diaChi NVARCHAR(200),
    ngaySinh DATE,
    trangThai BIT DEFAULT 1,
    CCCD VARCHAR(20),
    gioiTinh BIT DEFAULT 1
);

ALTER TABLE [User]
ADD chuyenMon NVARCHAR(100);

CREATE TABLE KhachVangLai (
    idKhachVangLai CHAR(7) PRIMARY KEY,
    hoVaTen NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15) NOT NULL UNIQUE,
    diaChi NVARCHAR(200),
	idPhuong CHAR(7) FOREIGN KEY REFERENCES Phuong(idPhuong), 
);

CREATE TABLE NhaSanXuat (
    idNSX CHAR(7) PRIMARY KEY,
    tenNSX NVARCHAR(100) NOT NULL
);

CREATE TABLE LoaiLinhKien (
    idLoaiLinhKien CHAR(7)  PRIMARY KEY,
    tenLoaiLinhKien NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE LinhKien (
    idLinhKien CHAR(7) PRIMARY KEY,
    idNSX CHAR(7) FOREIGN KEY REFERENCES NhaSanXuat(idNSX),
    idLoaiLinhKien CHAR(7) FOREIGN KEY REFERENCES LoaiLinhKien(idLoaiLinhKien),
    tenLinhKien NVARCHAR(100) NOT NULL,
    gia DECIMAL(18,2) NOT NULL CHECK (gia >= 0),
    soLuong CHAR(7) NOT NULL CHECK (soLuong >= 0),
    anh NVARCHAR(100), -- path
    thoiGianBaoHanh DATE NOT NULL,
    dieuKienBaoHanh NVARCHAR(500) NOT NULL
);

CREATE TABLE PhiLapDat (
    idPhiLapDat CHAR(7) PRIMARY KEY,
    idLinhKien CHAR(7) FOREIGN KEY REFERENCES LinhKien(idLinhKien),
    phi DECIMAL(18,2) NOT NULL CHECK (phi >= 0),
    ngayApDung DATE
);

CREATE TABLE AnhLinhKien (
    idAnh CHAR(7) PRIMARY KEY,
    idLinhKien CHAR(7) FOREIGN KEY REFERENCES LinhKien(idLinhKien),
    anh NVARCHAR(200) NOT NULL UNIQUE
);

CREATE TABLE ThietBi (
    idLoaiThietBi CHAR(7) PRIMARY KEY,
    tenLoaiThietBi NVARCHAR(200) NOT NULL UNIQUE
);

CREATE TABLE LoaiLoi (
    idLoi CHAR(7) PRIMARY KEY,
    moTaLoi NVARCHAR(200)
);

CREATE TABLE DonGia (
    idDonGia CHAR(7) PRIMARY KEY,
    idLoi CHAR(7) FOREIGN KEY REFERENCES LoaiLoi(idLoi),
    gia DECIMAL(18,2) NOT NULL CHECK(gia >= 0),
    ngayCapNhat DATE
);

CREATE TABLE DonDichVu (
    idDonDichVu CHAR(7) PRIMARY KEY,
    idUser CHAR(7) FOREIGN KEY REFERENCES [User](idUser),
    idKhachVangLai CHAR(7) FOREIGN KEY REFERENCES KhachVangLai(idKhachVangLai),
    idNhanVienKyThuat CHAR(7) FOREIGN KEY REFERENCES [User](idUser) NOT NULL,
    idUserTaoDon CHAR(7) FOREIGN KEY REFERENCES [User](idUser) NOT NULL,
	idLoaiThietBi CHAR(7) FOREIGN KEY REFERENCES ThietBi(idLoaiThietBi) NOT NULL,
	tenThietBi NVARCHAR(150),
    loaiKhachHang NVARCHAR(50) NOT NULL,
    ngayTaoDon DATETIME,
    ngayHoanThanh DATETIME,
    tongTien DECIMAL(18,2) CHECK (tongTien >= 0),
	loaiDonDichVu NVARCHAR(100) NOT NULL CHECK (loaiDonDichVu IN (N'Sửa chữa', N'Lắp đặt'))
);

CREATE TABLE ChiTietDonDichVu (
    idCTDH CHAR(7) PRIMARY KEY,
    idDonDichVu CHAR(7) FOREIGN KEY REFERENCES DonDichVu(idDonDichVu),
    idLinhKien CHAR(7) FOREIGN KEY REFERENCES LinhKien(idLinhKien),
    idLoi CHAR(7) FOREIGN KEY REFERENCES LoaiLoi(idLoi),
    moTa NVARCHAR(500),
    trangThai NVARCHAR(150) NOT NULL,
    soLuong INT NOT NULL CHECK (soLuong >= 0),
    ngayKetThucBH DATE,
    loaiDichVu NVARCHAR(100) NOT NULL CHECK (loaiDichVu IN (N'Sửa chữa', N'Lắp đặt')),
	thoiGianThemLinhKien DATETIME,
	hanBaoHanh BIT
);

ALTER TABLE ChiTietDonDichVu
ADD CONSTRAINT chk_ChiTietDonDichVu_LoaiDichVu
CHECK (
    (idLoi IS NOT NULL AND idLinhKien IS NULL)
    OR
    (idLoi IS NULL AND idLinhKien IS NOT NULL)
);


CREATE TABLE HinhAnh (
    idHinhAnh CHAR(7) PRIMARY KEY,
    idCTDH CHAR(7) FOREIGN KEY REFERENCES ChiTietDonDichVu(idCTDH),
    anh NVARCHAR(100) NOT NULL UNIQUE,
    loaiHinhAnh NVARCHAR(50) NOT NULL -- loại ảnh khách hàng: bảo hành/thiết bị linh kiện
);

CREATE TABLE DanhGia (
    idDanhGia CHAR(7) PRIMARY KEY,
    idDonDichVu CHAR(7) FOREIGN KEY REFERENCES DonDichVu(idDonDichVu),
    danhGiaNhanVien INT CHECK (danhGiaNhanVien BETWEEN 1 AND 5),
    danhGiaDichVu INT CHECK (danhGiaDichVu BETWEEN 1 AND 5),
    gopY TEXT
);

------------------------- INSERT

INSERT INTO ThanhPho (idThanhPho, tenThanhPho) VALUES
('TP001', N'Hà Nội'),
('TP002', N'Hồ Chí Minh'),
('TP003', N'Đà Nẵng'),
('TP004', N'Hải Phòng'),
('TP005', N'Cần Thơ'),
('TP006', N'Bắc Ninh'),
('TP007', N'Bình Dương'),
('TP008', N'Đồng Nai'),
('TP009', N'Hải Dương'),
('TP010', N'Thái Nguyên'),
('TP011', N'Nam Định'),
('TP012', N'Vinh'),
('TP013', N'Quảng Ninh'),
('TP014', N'Thanh Hóa'),
('TP015', N'Nghệ An'),
('TP016', N'Phú Thọ'),
('TP017', N'Thái Bình'),
('TP018', N'Quảng Nam'),
('TP019', N'Bình Định'),
('TP020', N'Đà Lạt');

INSERT INTO Quan (idQuan, idThanhPho, tenQuan) VALUES
('QHN001', 'TP001', N'Ba Đình'),
('QHN002', 'TP001', N'Hoàn Kiếm'),
('QHN003', 'TP001', N'Hai Bà Trưng'),
('QHN004', 'TP001', N'Đống Đa'),
('QHN005', 'TP001', N'Tây Hồ'),
('QHCM001', 'TP002', N'Quận 1'),
('QHCM002', 'TP002', N'Quận 3'),
('QHCM003', 'TP002', N'Quận 5'),
('QHCM004', 'TP002', N'Quận 10'),
('QHCM005', 'TP002', N'Gò Vấp'),
('QDN001', 'TP003', N'Hải Châu'),
('QDN002', 'TP003', N'Thanh Khê'),
('QDN003', 'TP003', N'Sơn Trà'),
('QDN004', 'TP003', N'Ngũ Hành Sơn'),
('QDN005', 'TP003', N'Liên Chiểu'),
('QHP001', 'TP004', N'Hồng Bàng'),
('QHP002', 'TP004', N'Ngô Quyền'),
('QHP003', 'TP004', N'Lê Chân'),
('QHP004', 'TP004', N'Kiến An'),
('QHP005', 'TP004', N'Dương Kinh');

INSERT INTO Phuong (idPhuong, idQuan, idThanhPho, tenPhuong) VALUES
('PHN001', 'QHN001', 'TP001', N'Phúc Xá'),
('PHN002', 'QHN001', 'TP001', N'Trúc Bạch'),
('PHN003', 'QHN001', 'TP001', N'Vĩnh Phúc'),
('PHN004', 'QHN002', 'TP001', N'Phan Chu Trinh'),
('PHN005', 'QHN002', 'TP001', N'Hàng Bài'),
('PHN006', 'QHN003', 'TP001', N'Bạch Mai'),
('PHN007', 'QHN003', 'TP001', N'Bùi Thị Xuân'),
('PHN008', 'QHN004', 'TP001', N'Chợ Dừa'),
('PHN009', 'QHN004', 'TP001', N'Khâm Thiên'),
('PHN010', 'QHN005', 'TP001', N'Xuân La'),
('PHCM001', 'QHCM001', 'TP002', N'Bến Nghé'),
('PHCM002', 'QHCM001', 'TP002', N'Bến Thành'),
('PHCM003', 'QHCM002', 'TP002', N'Võ Thị Sáu'),
('PHCM004', 'QHCM002', 'TP002', N'Phường 4'),
('PHCM005', 'QHCM003', 'TP002', N'Phường 1'),
('PHCM006', 'QHCM003', 'TP002', N'Phường 2'),
('PHCM007', 'QHCM004', 'TP002', N'Phường 12'),
('PHCM008', 'QHCM004', 'TP002', N'Phường 13'),
('PHCM009', 'QHCM005', 'TP002', N'Phường 3'),
('PHCM010', 'QHCM005', 'TP002', N'Phường 40');

INSERT INTO [Role] (idRole, tenRole) VALUES
('R001', N'Quản trị viên'),
('R002', N'Nhân viên kỹ thuật'),
('R003', N'Nhân viên chăm sóc khách hàng'),
('R004', N'Nhân viên quản lý'),
('R005', N'Khách hàng')

SET DATEFORMAT dmy;
-- Quản trị viên (R001)
INSERT INTO [User] (idUser, idRole, idPhuong, tenUser, hoVaTen, SDT, matKhau, diaChi, ngaySinh, CCCD, chuyenMon)
VALUES
('U000001', 'R001', 'PHN001', 'admin01', N'Nguyễn Văn A1', '0910000001', 'matkhau1', N'Địa chỉ 1', '1990-01-01', '001199000001', NULL),
('U000002', 'R001', 'PHN001', 'admin02', N'Nguyễn Văn A2', '0910000002', 'matkhau2', N'Địa chỉ 2', '1990-01-02', '001199000002', NULL),
('U000003', 'R001', 'PHN003', 'admin03', N'Nguyễn Văn A3', '0910000003', 'matkhau3', N'Địa chỉ 3', '1990-01-03', '001199000003', NULL),
('U000004', 'R001', 'PHN004', 'admin04', N'Nguyễn Văn A4', '0910000004', 'matkhau4', N'Địa chỉ 4', '1990-01-04', '001199000004', NULL),
('U000005', 'R001', 'PHN005', 'admin05', N'Nguyễn Văn A5', '0910000005', 'matkhau5', N'Địa chỉ 5', '1990-01-05', '001199000005', NULL);

-- Nhân viên kỹ thuật (R002)
INSERT INTO [User] (idUser, idRole, idPhuong, tenUser, hoVaTen, SDT, matKhau, diaChi, ngaySinh, CCCD, chuyenMon)
VALUES
('U000006', 'R002', 'PHN001', 'kithuat01', N'Trần Văn B1', '0920000001', 'matkhau6', N'Địa chỉ 6', '1991-01-01', '001199000006', N'Sửa chữa linh kiện cơ – điện'),
('U000007', 'R002', 'PHN005', 'kithuat02', N'Trần Văn B2', '0920000002', 'matkhau7', N'Địa chỉ 7', '1991-01-02', '001199000007', N'Sửa chữa nguồn xung / biến áp'),
('U000008', 'R002', 'PHN004', 'kithuat03', N'Trần Văn B3', '0920000003', 'matkhau8', N'Địa chỉ 8', '1991-01-03', '001199000008', N'Sửa chữa board mạch điều khiển'),
('U000009', 'R002', 'PHN005', 'kithuat04', N'Trần Văn B4', '0920000004', 'matkhau9', N'Địa chỉ 9', '1991-01-04', '001199000009', N'Sửa chữa thiết bị mạng'),
('U000010', 'R002', 'PHN003', 'kithuat05', N'Trần Văn B5', '0920000005', 'matkhau10', N'Địa chỉ 10', '1991-01-05', '001199000010', N' Sửa chữa mạch điện tử');

-- Nhân viên CSKH (R003)
INSERT INTO [User] (idUser, idRole, idPhuong, tenUser, hoVaTen, SDT, matKhau, diaChi, ngaySinh, CCCD, chuyenMon)
VALUES
('U000011', 'R003', 'PHN005', 'cskh01', N'Lê Thị C1', '0930000001', 'matkhau11', N'Địa chỉ 11', '1992-01-01', '001199000011', NULL),
('U000012', 'R003', 'PHN002', 'cskh02', N'Lê Thị C2', '0930000002', 'matkhau12', N'Địa chỉ 12', '1992-01-02', '001199000012', NULL),
('U000013', 'R003', 'PHN005', 'cskh03', N'Lê Thị C3', '0930000003', 'matkhau13', N'Địa chỉ 13', '1992-01-03', '001199000013', NULL),
('U000014', 'R003', 'PHN003', 'cskh04', N'Lê Thị C4', '0930000004', 'matkhau14', N'Địa chỉ 14', '1992-01-04', '001199000014', NULL),
('U000015', 'R003', 'PHN004', 'cskh05', N'Lê Thị C5', '0930000005', 'matkhau15', N'Địa chỉ 15', '1992-01-05', '001199000015', NULL);

-- Nhân viên quản lý (R004)
INSERT INTO [User] (idUser, idRole, idPhuong, tenUser, hoVaTen, SDT, matKhau, diaChi, ngaySinh, CCCD, chuyenMon)
VALUES
('U000016', 'R004', 'PHN003', 'ql01', N'Phạm Văn D1', '0940000001', 'matkhau16', N'Địa chỉ 16', '1989-01-01', '001199000016', NULL),
('U000017', 'R004', 'PHN003', 'ql02', N'Phạm Văn D2', '0940000002', 'matkhau17', N'Địa chỉ 17', '1989-01-02', '001199000017', NULL),
('U000018', 'R004', 'PHN005', 'ql03', N'Phạm Văn D3', '0940000003', 'matkhau18', N'Địa chỉ 18', '1989-01-03', '001199000018', NULL),
('U000019', 'R004', 'PHN004', 'ql04', N'Phạm Văn D4', '0940000004', 'matkhau19', N'Địa chỉ 19', '1989-01-04', '001199000019', NULL),
('U000020', 'R004', 'PHN005', 'ql05', N'Phạm Văn D5', '0940000005', 'matkhau20', N'Địa chỉ 20', '1989-01-05', '001199000020', NULL);

-- Khách hàng (R005) - 10 người
INSERT INTO [User] (idUser, idRole, idPhuong, tenUser, hoVaTen, SDT, matKhau, diaChi, ngaySinh, CCCD, chuyenMon)
VALUES
('U000021', 'R005', 'PHN006', 'kh01', N'Khách 1', '0950000001', 'matkhau21', N'Địa chỉ KH1', '1993-01-01', '001199000021', NULL),
('U000022', 'R005', 'PHN006', 'kh02', N'Khách 2', '0950000002', 'matkhau22', N'Địa chỉ KH2', '1993-01-02', '001199000022', NULL),
('U000023', 'R005', 'PHN006', 'kh03', N'Khách 3', '0950000003', 'matkhau23', N'Địa chỉ KH3', '1993-01-03', '001199000023', NULL),
('U000024', 'R005', 'PHN007', 'kh04', N'Khách 4', '0950000004', 'matkhau24', N'Địa chỉ KH4', '1993-01-04', '001199000024', NULL),
('U000025', 'R005', 'PHN007', 'kh05', N'Khách 5', '0950000005', 'matkhau25', N'Địa chỉ KH5', '1993-01-05', '001199000025', NULL),
('U000026', 'R005', 'PHN008', 'kh06', N'Khách 6', '0950000006', 'matkhau26', N'Địa chỉ KH6', '1993-01-06', '001199000026', NULL),
('U000027', 'R005', 'PHN008', 'kh07', N'Khách 7', '0950000007', 'matkhau27', N'Địa chỉ KH7', '1993-01-07', '001199000027', NULL),
('U000028', 'R005', 'PHN009', 'kh08', N'Khách 8', '0950000008', 'matkhau28', N'Địa chỉ KH8', '1993-01-08', '001199000028', NULL),
('U000029', 'R005', 'PHN009', 'kh09', N'Khách 9', '0950000009', 'matkhau29', N'Địa chỉ KH9', '1993-01-09', '001199000029', NULL),
('U000030', 'R005', 'PHN007', 'kh10', N'Khách 10', '0950000010', 'matkhau30', N'Địa chỉ KH10', '1993-01-10', '001199000030', NULL);

INSERT INTO KhachVangLai (idKhachVangLai, hoVaTen, SDT, diaChi, idPhuong) VALUES
('KVL001', N'Trần Phước Lộc', '0912345678', N'Số 1 Ngõ 1 Phúc Xá', 'PHN001'),
('KVL002', N'Lưu Ngọc Yến Như', '0912345679', N'Số 2 Ngõ 2 Trúc Bạch', 'PHN002'),
('KVL003', N'Nguyễn Vũ Khanh', '0912345680', N'Số 3 Ngõ 3 Vĩnh Phúc', 'PHN003'),
('KVL004', N'Phạm Thị Nước', '0912345681', N'Số 4 Ngõ 4 Phan Chu Trinh', 'PHN004'),
('KVL005', N'Hoàng Văn Lạnh', '0912345682', N'Số 5 Ngõ 5 Hàng Bài', 'PHN005'),
('KVL006', N'Vũ Thị Điện', '0912345683', N'Số 6 Ngõ 6 Bạch Mai', 'PHN006'),
('KVL007', N'Đặng Văn Gia', '0912345684', N'Số 7 Ngõ 7 Bùi Thị Xuân', 'PHN007'),
('KVL008', N'Bùi Thị Công', '0912345685', N'Số 8 Ngõ 8 Chợ Dừa', 'PHN008'),
('KVL009', N'Mai Văn Trời', '0912345686', N'Số 9 Ngõ 9 Khâm Thiên', 'PHN009'),
('KVL010', N'Lý Thị Nước', '0912345687', N'Số 10 Ngõ 10 Xuân La', 'PHN010'),
('KVL011', N'Chu Văn Hòa', '0912345688', N'Số 11 Ngõ 11 Bến Nghé', 'PHCM001'),
('KVL012', N'Trương Thị Lạnh', '0912345689', N'Số 12 Ngõ 12 Bến Thành', 'PHCM002'),
('KVL013', N'Đỗ Văn Máy', '0912345690', N'Số 13 Ngõ 13 Võ Thị Sáu', 'PHCM003'),
('KVL014', N'Ngô Thị Bình', '0912345691', N'Số 14 Ngõ 14 Phường 4', 'PHCM004'),
('KVL015', N'Hồ Văn Điện', '0912345692', N'Số 15 Ngõ 15 Phường 1', 'PHCM005'),
('KVL016', N'Phan Thị Chiếu', '0912345693', N'Số 16 Ngõ 16 Phường 2', 'PHCM006'),
('KVL017', N'Vương Văn Loa', '0912345694', N'Số 17 Ngõ 17 Phường 12', 'PHCM007'),
('KVL018', N'Lưu Thị Như', '0912345695', N'Số 18 Ngõ 18 Phường 13', 'PHCM008'),
('KVL019', N'Đinh Văn Tuấn', '0912345696', N'Số 19 Ngõ 19 Phường 3', 'PHCM009'),
('KVL020', N'Lâm Thị Hoa', '0912345697', N'Số 20 Ngõ 20 Phường 4', 'PHCM010');

INSERT INTO NhaSanXuat (idNSX, tenNSX) VALUES
('NSX001', N'Samsung'),
('NSX002', N'LG'),
('NSX003', N'Panasonic'),
('NSX004', N'Toshiba'),
('NSX005', N'Sharp'),
('NSX006', N'Sony'),
('NSX007', N'Electrolux'),
('NSX008', N'Aqua'),
('NSX009', N'Daikin'),
('NSX010', N'Media'),
('NSX011', N'Kangaroo'),
('NSX012', N'Sunhouse'),
('NSX013', N'Philips'),
('NSX014', N'Beko'),
('NSX015', N'Mitsubishi'),
('NSX016', N'Funiki'),
('NSX017', N'National'),
('NSX018', N'Hitachi'),
('NSX019', N'Casio'),
('NSX020', N'Asanzo');

INSERT INTO LoaiLinhKien (idLoaiLinhKien, tenLoaiLinhKien) VALUES
('LLK001', N'Tụ điện'),
('LLK002', N'Điện trở'),
('LLK003', N'Cuộn cảm'),
('LLK004', N'Diode'),
('LLK005', N'Triac / Thyristor'),
('LLK006', N'MOSFET / Transistor'),
('LLK007', N'IC nguồn'),
('LLK008', N'IC điều khiển'),
('LLK009', N'Rơ-le (Relay)'),
('LLK010', N'Cảm biến nhiệt'),
('LLK011', N'Cảm biến dòng / áp'),
('LLK012', N'Mạch nguồn xung'),
('LLK013', N'Mạch inverter'),
('LLK014', N'Mạch điều khiển vi xử lý'),
('LLK015', N'Chân cắm / Header / Socket'),
('LLK016', N'Jack nguồn / Audio / HDMI'),
('LLK017', N'Motor điện DC / AC'),
('LLK018', N'Board mạch điện tử'),
('LLK019', N'Mạch sạc / pin'),
('LLK020', N'Cầu chì / Bảo vệ mạch');

INSERT INTO LinhKien (idLinhKien, idNSX, idLoaiLinhKien, tenLinhKien, gia, soLuong, anh, thoiGianBaoHanh, dieuKienBaoHanh) VALUES
('LK001', 'NSX001', 'LLK001', N'Tụ điện 450V 50uF', 25000, '300', 'tu_dien_450v.jpg', '2025-12-31', N'Bảo hành 12 tháng'),
('LK002', 'NSX001', 'LLK002', N'Điện trở công suất 5W 220Ω', 5000, '500', 'dien_tro_5w.jpg', '2025-12-31', N'Không đổi trả sau hư hỏng do quá tải'),
('LK003', 'NSX002', 'LLK003', N'Cuộn cảm 10mH 5A lõi ferrite', 18000, '250', 'cuon_cam_10mh.jpg', '2025-12-31', N'Bảo hành 6 tháng'),
('LK004', 'NSX002', 'LLK004', N'Diode Schottky 1N5819', 3000, '1000', 'diode.jpg', '2025-12-31', N'Không bảo hành do linh kiện nhỏ'),
('LK005', 'NSX003', 'LLK005', N'Triac BTA16-600B', 9000, '400', 'triac.jpg', '2025-12-31', N'Bảo hành 3 tháng'),
('LK006', 'NSX003', 'LLK006', N'MOSFET IRF540N 100V 33A', 12000, '600', 'mosfet.jpg', '2025-12-31', N'Không bảo hành nếu chân gãy'),
('LK007', 'NSX004', 'LLK007', N'IC nguồn LNK304PN', 15000, '350', 'ic_lnk.jpg', '2025-12-31', N'Bảo hành 6 tháng'),
('LK008', 'NSX004', 'LLK008', N'IC vi điều khiển ATmega328P', 40000, '200', 'ic_atmega.jpg', '2025-12-31', N'Không bảo hành khi chân bị hàn lỗi'),
('LK009', 'NSX005', 'LLK009', N'Relay 12VDC 10A', 18000, '450', 'relay_12v.jpg', '2025-12-31', N'Bảo hành 6 tháng'),
('LK010', 'NSX005', 'LLK010', N'Cảm biến nhiệt độ NTC 10K', 6000, '700', 'cam_bien_ntc.jpg', '2025-12-31', N'Không bảo hành'),
('LK011', 'NSX006', 'LLK011', N'Cảm biến dòng ACS712 20A', 35000, '300', 'cam_bien_acs.jpg', '2025-12-31', N'Bảo hành 3 tháng'),
('LK012', 'NSX006', 'LLK012', N'Mạch nguồn xung 5V 2A mini', 48000, '150', 'mach_nguon_5v.jpg', '2025-12-31', N'Bảo hành 12 tháng'),
('LK013', 'NSX007', 'LLK013', N'Mạch inverter 220V LED', 65000, '100', 'mach_led.jpg', '2025-12-31', N'Bảo hành 6 tháng'),
('LK014', 'NSX007', 'LLK014', N'Mạch điều khiển vi xử lý STM32', 98000, '120', 'mach_stm32.jpg', '2025-12-31', N'Bảo hành 6 tháng'),
('LK015', 'NSX008', 'LLK015', N'Socket DIP 28 chân', 3000, '1000', 'socket_dip.jpg', '2025-12-31', N'Không bảo hành'),
('LK016', 'NSX008', 'LLK016', N'Jack nguồn DC 5.5mm', 2000, '1200', 'jack_dc.jpg', '2025-12-31', N'Không bảo hành'),
('LK017', 'NSX009', 'LLK017', N'Motor DC 12V 200rpm', 50000, '160', 'motor_12v.jpg', '2025-12-31', N'Bảo hành 6 tháng'),
('LK018', 'NSX009', 'LLK018', N'Board mạch điện tử đa năng', 75000, '140', 'board.jpg', '2025-12-31', N'Bảo hành 12 tháng'),
('LK019', 'NSX010', 'LLK019', N'Mạch sạc pin lithium 3.7V TP4056', 12000, '400', 'mach_tp.jpg', '2025-12-31', N'Không bảo hành'),
('LK020', 'NSX010', 'LLK020', N'Cầu chì 5A 250V chân cắm', 4000, '600', 'cauchi_5a.jpg', '2025-12-31', N'Không bảo hành');

INSERT INTO PhiLapDat (idPhiLapDat, idLinhKien, phi, ngayApDung) VALUES
('PLD001', 'LK001', 500000, '2023-01-01'),
('PLD002', 'LK002', 200000, '2023-01-01'),
('PLD003', 'LK003', 450000, '2023-01-01'),
('PLD004', 'LK004', 150000, '2023-01-01'),
('PLD005', 'LK005', 350000, '2023-01-01'),
('PLD006', 'LK006', 180000, '2023-01-01'),
('PLD007', 'LK007', 250000, '2023-01-01'),
('PLD008', 'LK008', 100000, '2023-01-01'),
('PLD009', 'LK009', 120000, '2023-01-01'),
('PLD010', 'LK010', 80000, '2023-01-01'),
('PLD011', 'LK011', 600000, '2023-01-01'),
('PLD012', 'LK012', 50000, '2023-01-01'),
('PLD013', 'LK013', 150000, '2023-01-01'),
('PLD014', 'LK014', 100000, '2023-01-01'),
('PLD015', 'LK015', 120000, '2023-01-01'),
('PLD016', 'LK016', 300000, '2023-01-01'),
('PLD017', 'LK017', 700000, '2023-01-01'),
('PLD018', 'LK018', 650000, '2023-01-01'),
('PLD019', 'LK019', 200000, '2023-01-01'),
('PLD020', 'LK020', 350000, '2023-01-01');

INSERT INTO AnhLinhKien (idAnh, idLinhKien, anh) VALUES
('AL001', 'LK001', 'block_samsung_1.jpg'),
('AL002', 'LK001', 'block_samsung_2.jpg'),
('AL003', 'LK002', 'quat_gio_samsung_1.jpg'),
('AL004', 'LK002', 'quat_gio_samsung_2.jpg'),
('AL005', 'LK003', 'block_lg_80w_1.jpg'),
('AL006', 'LK003', 'block_lg_80w_2.jpg'),
('AL007', 'LK004', 'quat_dan_lanh_lg_1.jpg'),
('AL008', 'LK004', 'quat_dan_lanh_lg_2.jpg'),
('AL009', 'LK005', 'moto_panasonic_1.jpg'),
('AL010', 'LK005', 'moto_panasonic_2.jpg'),
('AL011', 'LK006', 'bomxa_panasonic_1.jpg'),
('AL012', 'LK006', 'bomxa_panasonic_2.jpg'),
('AL013', 'LK007', 'thanh_dot_toshiba_1.jpg'),
('AL014', 'LK007', 'thanh_dot_toshiba_2.jpg'),
('AL015', 'LK008', 'role_toshiba_1.jpg'),
('AL016', 'LK008', 'role_toshiba_2.jpg'),
('AL017', 'LK009', 'moto_quat_1.jpg'),
('AL018', 'LK009', 'moto_quat_2.jpg'),
('AL019', 'LK010', 'canh_quat_1.jpg'),
('AL020', 'LK010', 'canh_quat_2.jpg');

INSERT INTO ThietBi (idLoaiThietBi, tenLoaiThietBi) VALUES
('TB001', N'Laptop'),
('TB002', N'Máy tính để bàn (PC)'),
('TB003', N'Màn hình máy tính'),
('TB004', N'Bàn phím'),
('TB005', N'Chuột máy tính'),
('TB006', N'Loa vi tính'),
('TB007', N'Tai nghe'),
('TB008', N'Ổ cứng SSD'),
('TB009', N'Ổ cứng HDD'),
('TB010', N'RAM máy tính'),
('TB011', N'Mainboard'),
('TB012', N'Card màn hình (GPU)'),
('TB013', N'Nguồn máy tính (PSU)'),
('TB014', N'Vỏ case máy tính'),
('TB015', N'Máy in'),
('TB016', N'Máy scan'),
('TB017', N'Router WiFi'),
('TB018', N'Camera giám sát'),
('TB019', N'Webcam'),
('TB020', N'Bộ lưu điện (UPS)');

---------- TABLE LoaiLoi
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L021', N'Laptop không lên hình'),
('L022', N'Laptop bị đen màn hình'),
('L023', N'Laptop quá nóng'),
('L024', N'Laptop không nhận bàn phím');

-- Máy tính để bàn (PC) errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L025', N'PC không lên màn hình'),
('L026', N'PC không nhận chuột'),
('L027', N'PC không nhận bàn phím');

-- Màn hình máy tính errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L028', N'Màn hình không lên'),
('L029', N'Màn hình bị nhòe hình');

-- Bàn phím errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L030', N'Bàn phím bị liệt phím'),
('L031', N'Bàn phím không nhận phím');

-- Chuột máy tính errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L032', N'Chuột không nhận tín hiệu'),
('L033', N'Chuột bị trượt');

-- Loa vi tính errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L034', N'Loa không phát âm thanh'),
('L035', N'Loa bị rè');

-- Tai nghe errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L036', N'Tai nghe không phát ra âm thanh'),
('L037', N'Tai nghe bị rè');

-- Ổ cứng SSD/HDD errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L038', N'Ổ cứng không nhận'),
('L039', N'Ổ cứng bị hỏng');

-- RAM máy tính errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L040', N'RAM không nhận'),
('L041', N'RAM bị lỗi');

-- Mainboard errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L042', N'Mainboard không nhận tín hiệu'),
('L043', N'Mainboard bị lỗi');

-- Card màn hình (GPU) errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L044', N'Card màn hình không hiển thị'),
('L045', N'Card màn hình bị cháy');

-- Nguồn máy tính (PSU) errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L046', N'Nguồn không lên điện'),
('L047', N'Nguồn bị cháy');

-- Vỏ case máy tính errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L048', N'Vỏ case bị hỏng'),
('L049', N'Vỏ case không mở được');

-- Máy in errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L050', N'Máy in không in được'),
('L051', N'Máy in bị kẹt giấy');

-- Máy scan errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L052', N'Máy scan không quét được'),
('L053', N'Máy scan bị đen');

-- Router WiFi errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L054', N'Router không phát tín hiệu WiFi'),
('L055', N'Router bị mất kết nối');

-- Camera giám sát errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L056', N'Camera không ghi hình'),
('L057', N'Camera bị mờ hình');

-- Webcam errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L058', N'Webcam không nhận'),
('L059', N'Webcam không sáng');

-- Bộ lưu điện (UPS) errors
INSERT INTO LoaiLoi (idLoi, moTaLoi) VALUES
('L060', N'UPS không sạc được'),
('L061', N'UPS không cấp điện');
-------------------------------------------

INSERT INTO DonGia (idDonGia, idLoi, gia, ngayCapNhat) VALUES
-- Laptop errors
('DG021', 'L021', 450000, '2023-01-01'),
('DG022', 'L022', 500000, '2023-01-01'),
('DG023', 'L023', 350000, '2023-01-01'),
('DG024', 'L024', 250000, '2023-01-01'),

-- Máy tính để bàn (PC) errors
('DG025', 'L025', 400000, '2023-01-01'),
('DG026', 'L026', 200000, '2023-01-01'),
('DG027', 'L027', 250000, '2023-01-01'),

-- Màn hình máy tính errors
('DG028', 'L028', 350000, '2023-01-01'),
('DG029', 'L029', 400000, '2023-01-01'),

-- Bàn phím errors
('DG030', 'L030', 150000, '2023-01-01'),
('DG031', 'L031', 200000, '2023-01-01'),

-- Chuột máy tính errors
('DG032', 'L032', 100000, '2023-01-01'),
('DG033', 'L033', 120000, '2023-01-01'),

-- Loa vi tính errors
('DG034', 'L034', 250000, '2023-01-01'),
('DG035', 'L035', 300000, '2023-01-01'),

-- Tai nghe errors
('DG036', 'L036', 200000, '2023-01-01'),
('DG037', 'L037', 220000, '2023-01-01'),

-- Ổ cứng SSD/HDD errors
('DG038', 'L038', 500000, '2023-01-01'),
('DG039', 'L039', 600000, '2023-01-01'),

-- RAM máy tính errors
('DG040', 'L040', 250000, '2023-01-01'),
('DG041', 'L041', 300000, '2023-01-01'),

-- Mainboard errors
('DG042', 'L042', 700000, '2023-01-01'),
('DG043', 'L043', 800000, '2023-01-01'),

-- Card màn hình (GPU) errors
('DG044', 'L044', 750000, '2023-01-01'),
('DG045', 'L045', 900000, '2023-01-01'),

-- Nguồn máy tính (PSU) errors
('DG046', 'L046', 350000, '2023-01-01'),
('DG047', 'L047', 400000, '2023-01-01'),

-- Vỏ case máy tính errors
('DG048', 'L048', 200000, '2023-01-01'),
('DG049', 'L049', 250000, '2023-01-01'),

-- Máy in errors
('DG050', 'L050', 450000, '2023-01-01'),
('DG051', 'L051', 500000, '2023-01-01'),

-- Máy scan errors
('DG052', 'L052', 400000, '2023-01-01'),
('DG053', 'L053', 350000, '2023-01-01'),

-- Router WiFi errors
('DG054', 'L054', 250000, '2023-01-01'),
('DG055', 'L055', 300000, '2023-01-01'),

-- Camera giám sát errors
('DG056', 'L056', 500000, '2023-01-01'),
('DG057', 'L057', 550000, '2023-01-01'),

-- Webcam errors
('DG058', 'L058', 150000, '2023-01-01'),
('DG059', 'L059', 180000, '2023-01-01'),

-- Bộ lưu điện (UPS) errors
('DG060', 'L060', 350000, '2023-01-01'),
('DG061', 'L061', 400000, '2023-01-01');

INSERT INTO DonDichVu (idDonDichVu, idUser, idKhachVangLai, idNhanVienKyThuat, idUserTaoDon, idLoaiThietBi, tenThietBi, loaiKhachHang, ngayTaoDon, ngayHoanThanh, tongTien, loaiDonDichVu) VALUES
('DDV001', 'NVKT01', 'KVL001', 'NVKT01', 'NVKT01', 'TB001', N'Điều hòa Samsung 1 HP', N'Khách vãng lai', '2023-05-01 08:00:00', '2023-05-01 10:30:00', 3500000, N'Sửa chữa'),
('DDV002', 'NVKT02', 'KVL002', 'NVKT02', 'NVKT02', 'TB002', N'Tủ lạnh LG 300L', N'Khách vãng lai', '2023-05-02 09:00:00', '2023-05-02 11:15:00', 2250000, N'Sửa chữa'),
('DDV003', 'NVKT03', 'KVL003', 'NVKT03', 'NVKT03', 'TB003', N'Máy giặt Panasonic 8kg', N'Khách vãng lai', '2023-05-03 10:00:00', '2023-05-03 12:30:00', 1750000, N'Sửa chữa'),
('DDV004', 'NVKT04', 'KVL004', 'NVKT04', 'NVKT04', 'TB004', N'Bình nóng lạnh Toshiba 15L', N'Khách vãng lai', '2023-05-04 11:00:00', '2023-05-04 13:45:00', 1750000, N'Sửa chữa')
