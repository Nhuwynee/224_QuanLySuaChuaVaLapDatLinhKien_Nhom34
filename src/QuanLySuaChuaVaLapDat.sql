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
    soLuong INT NOT NULL CHECK (soLuong >= 0),
    anh NVARCHAR(100), -- path
    thoiGianBaoHanh INT NOT NULL,
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
	hinhThucDichVu NVARCHAR(100) NOT NULL CHECK (hinhThucDichVu in (N'Tại nhà', N'Trực tiếp')),
	loaiDonDichVu NVARCHAR(100) NOT NULL CHECK (loaiDonDichVu IN (N'Sửa chữa', N'Lắp đặt')),
	phuongThucThanhToan NVARCHAR(100),
    trangThaiDon NVARCHAR(150) NOT NULL,
	ngayChinhSua DATETIME
);

CREATE TABLE ChiTietDonDichVu (
    idCTDH CHAR(7) PRIMARY KEY,
    idDonDichVu CHAR(7) FOREIGN KEY REFERENCES DonDichVu(idDonDichVu),
    idLinhKien CHAR(7) FOREIGN KEY REFERENCES LinhKien(idLinhKien),
    idLoi CHAR(7) FOREIGN KEY REFERENCES LoaiLoi(idLoi),
	loaiDichVu NVARCHAR(100) NULL CHECK (loaiDichVu IN (N'Sửa chữa', N'Lắp đặt')),
    moTa NVARCHAR(500),
    soLuong INT NOT NULL CHECK (soLuong >= 0),
    ngayKetThucBH DATE, 
	thoiGianThemLinhKien DATETIME,
	hanBaoHanh BIT
);

--- thêm cột phuongThucThanhToan
--ALTER TABLE DonDichVu
--ADD phuongThucThanhToan NVARCHAR(100);

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
    gopY NTEXT
);


select * from dbo.DonDichVu

update dbo.DonDichVu
set trangThaiDon =N'Đang sửa chữa'
where idDonDichVu ='DDV001'
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

INSERT INTO LinhKien (idLinhKien, idNSX, idLoaiLinhKien, tenLinhKien, gia, soLuong, thoiGianBaoHanh, dieuKienBaoHanh) VALUES
('LK001', 'NSX001', 'LLK001', N'Tụ điện 450V 50uF', 25000, 300, 3, N'Bảo hành lỗi từ nhà sản xuất hoặc hư hỏng do vận chuyển'),
('LK002', 'NSX001', 'LLK002', N'Điện trở công suất 5W 220Ω', 5000, 500, 6, N'Bảo hành lỗi kỹ thuật hoặc sử dụng sai điện áp'),
('LK003', 'NSX002', 'LLK003', N'Cuộn cảm 10mH 5A lõi ferrite', 18000, 250, 9, N'Bảo hành lỗi vật liệu hoặc môi trường ẩm ướt'),
('LK004', 'NSX002', 'LLK004', N'Diode Schottky 1N5819', 3000, 1000, 12, N'Bảo hành lỗi sản xuất hoặc quá nhiệt'),
('LK005', 'NSX003', 'LLK005', N'Triac BTA16-600B', 9000, 400, 15, N'Bảo hành lỗi thiết kế hoặc hư hỏng do ngắn mạch'),
('LK006', 'NSX003', 'LLK006', N'MOSFET IRF540N 100V 33A', 12000, 600, 18, N'Bảo hành lỗi từ nhà sản xuất hoặc va đập'),
('LK007', 'NSX004', 'LLK007', N'IC nguồn LNK304PN', 15000, 350, 24, N'Bảo hành lỗi chip hoặc nguồn điện không ổn định'),
('LK008', 'NSX004', 'LLK008', N'IC vi điều khiển ATmega328P', 40000, 200, 3, N'Bảo hành lỗi lập trình hoặc hư hỏng chân hàn'),
('LK009', 'NSX005', 'LLK009', N'Relay 12VDC 10A', 18000, 450, 12, N'Bảo hành lỗi cơ học hoặc quá tải'),
('LK010', 'NSX005', 'LLK010', N'Cảm biến nhiệt độ NTC 10K', 6000, 700, 6, N'Bảo hành lỗi cảm biến hoặc môi trường khắc nghiệt'),
('LK011', 'NSX006', 'LLK011', N'Cảm biến dòng ACS712 20A', 35000, 300, 7, N'Bảo hành lỗi đo lường hoặc hư hỏng do từ trường'),
('LK012', 'NSX006', 'LLK012', N'Mạch nguồn xung 5V 2A mini', 48000, 150, 3, N'Bảo hành lỗi linh kiện hoặc điện áp không đúng'),
('LK013', 'NSX007', 'LLK013', N'Mạch inverter 220V LED', 65000, 100, 12, N'Bảo hành lỗi biến áp hoặc sử dụng sai tải'),
('LK014', 'NSX007', 'LLK014', N'Mạch điều khiển vi xử lý STM32', 98000, 120, 12, N'Bảo hành lỗi phần mềm hoặc hư hỏng do sét đánh'),
('LK015', 'NSX008', 'LLK015', N'Socket DIP 28 chân', 3000, 1000, 0, N'Không bảo hành do linh kiện tiêu hao'),
('LK016', 'NSX008', 'LLK016', N'Jack nguồn DC 5.5mm', 2000, 1200, 0, N'Không bảo hành do hư hỏng vật lý'),
('LK017', 'NSX009', 'LLK017', N'Motor DC 12V 200rpm', 50000, 160, 12, N'Bảo hành lỗi động cơ hoặc quá tải'),
('LK018', 'NSX009', 'LLK018', N'Board mạch điện tử đa năng', 75000, 140, 9, N'Bảo hành lỗi hàn hoặc môi trường bụi bẩn'),
('LK019', 'NSX010', 'LLK019', N'Mạch sạc pin lithium 3.7V TP4056', 12000, 400, 6, N'Bảo hành lỗi mạch hoặc pin bị phồng'),
('LK020', 'NSX010', 'LLK020', N'Cầu chì 5A 250V chân cắm', 4000, 600, 0, N'Không bảo hành do cháy nổ');
GO


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



----------------------- UPDATE lại idUser trong bảng User (có chạy)
-- SELECT * FROM [USER]

update [User] 
set IdUser = REPLACE(idUser, 'U0', 'AD')
where idRole = 'R001' AND idUser like 'U0%'

update [User] 
set IdUser = REPLACE(idUser, 'U000', 'NVKT')
where idRole = 'R002' AND idUser like 'U000%'

update [User] 
set IdUser = REPLACE(idUser, 'U000', 'CSKH')
where idRole = 'R003' AND idUser like 'U000%'

update [User] 
set IdUser = REPLACE(idUser, 'U0', 'QL')
where idRole = 'R004' AND idUser like 'U0%'

update [User] 
set IdUser = REPLACE(idUser, 'U0', 'KH')
where idRole = 'R005' AND idUser like 'U0%'

-- Thêm 10 đơn dịch vụ mới
INSERT INTO DonDichVu (idDonDichVu, idUser, idKhachVangLai, idNhanVienKyThuat, idUserTaoDon, idLoaiThietBi, tenThietBi, loaiKhachHang, ngayTaoDon, ngayHoanThanh, tongTien, hinhThucDichVu, loaiDonDichVu, phuongThucThanhToan, trangThaiDon, ngayChinhSua)
VALUES
-- Đơn 1: Khách hàng thường (KH001), NV kỹ thuật NVKT01
('DDV001', 'KH00021', NULL, 'NVKT006', 'CSKH011', 'TB001', N'Laptop Dell Inspiron', N'Khách hàng thường', '2025-02-01 09:00:00', '2025-02-01 11:30:00', 1200000, N'Tại nhà', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-01 11:30:00'),

-- Đơn 2: Khách vãng lai, NV kỹ thuật NVKT02
('DDV002', NULL, 'KVL001', 'NVKT007', 'CSKH012', 'TB002', N'PC Asus Gaming', N'Khách vãng lai', '2025-02-02 10:00:00', '2025-02-02 12:45:00', 1850000, N'Trực tiếp', N'Sửa chữa', N'Chuyển khoản', N'Hoàn thành', '2025-02-02 12:45:00'),

-- Đơn 3: Khách hàng thường (KH002), NV kỹ thuật NVKT03
('DDV003', 'KH00022', NULL, 'NVKT008', 'CSKH013', 'TB003', N'Màn hình LG 24inch', N'Khách hàng thường', '2025-02-03 08:30:00', '2025-02-03 10:15:00', 650000, N'Tại nhà', N'Lắp đặt', N'Tiền mặt', N'Hoàn thành', '2025-02-03 10:15:00'),

-- Đơn 4: Khách vãng lai, NV kỹ thuật NVKT04
('DDV004', NULL, 'KVL002', 'NVKT009', 'CSKH014', 'TB004', N'Bàn phím cơ', N'Khách vãng lai', '2025-02-04 14:00:00', '2025-02-04 15:30:00', 350000, N'Trực tiếp', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-04 15:30:00'),

-- Đơn 5: Khách hàng thường (KH003), NV kỹ thuật NVKT05
('DDV005', 'KH00023', NULL, 'NVKT010', 'CSKH015', 'TB005', N'Chuột không dây', N'Khách hàng thường', '2025-02-05 11:00:00', '2025-02-05 12:00:00', 250000, N'Tại nhà', N'Sửa chữa', N'Chuyển khoản', N'Hoàn thành', '2025-02-05 12:00:00'),

-- Đơn 6: Khách vãng lai, NV kỹ thuật NVKT01
('DDV006', NULL, 'KVL003', 'NVKT006', 'CSKH011', 'TB006', N'Loa vi tính', N'Khách vãng lai', '2025-02-06 13:30:00', '2025-02-06 15:00:00', 500000, N'Trực tiếp', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-06 15:00:00'),

-- Đơn 7: Khách hàng thường (KH004), NV kỹ thuật NVKT02
('DDV007', 'KH00024', NULL, 'NVKT007', 'CSKH012', 'TB007', N'Tai nghe Bluetooth', N'Khách hàng thường', '2025-02-07 09:15:00', '2025-02-07 10:45:00', 450000, N'Tại nhà', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-07 10:45:00'),

-- Đơn 8: Khách vãng lai, NV kỹ thuật NVKT03
('DDV008', NULL, 'KVL004', 'NVKT006', 'CSKH013', 'TB008', N'Ổ cứng SSD 512GB', N'Khách vãng lai', '2025-02-08 10:30:00', '2025-02-08 12:00:00', 1200000, N'Trực tiếp', N'Lắp đặt', N'Chuyển khoản', N'Hoàn thành', '2025-02-08 12:00:00'),

-- Đơn 9: Khách hàng thường (KH005), NV kỹ thuật NVKT04
('DDV009', 'KH00025', NULL, 'NVKT008', 'CSKH014', 'TB009', N'Ổ cứng HDD 1TB', N'Khách hàng thường', '2025-02-09 14:00:00', '2025-02-09 15:30:00', 800000, N'Tại nhà', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-09 15:30:00'),

-- Đơn 10: Khách vãng lai, NV kỹ thuật NVKT05
('DDV010', NULL, 'KVL005', 'NVKT010', 'CSKH015', 'TB010', N'RAM DDR4 8GB', N'Khách vãng lai', '2025-02-10 11:00:00', '2025-02-10 12:30:00', 600000, N'Trực tiếp', N'Lắp đặt', N'Chuyển khoản', N'Hoàn thành', '2025-02-10 12:30:00'),

-- Đơn 11: Khách vãng lai, NV kỹ thuật NVKT05
('DDV011', NULL, 'KVL005', 'NVKT010', 'CSKH015', 'TB010', N'RAM DDR4 8GB', N'Khách vãng lai', '2025-02-10 11:00:00', '2025-02-10 12:30:00', 600000, N'Trực tiếp', N'Lắp đặt', N'Chuyển khoản', N'Đang sửa chữa', '2025-02-10 12:30:00');
select * from ChiTietDonDichVu
select * from LinhKien
-- Thêm chi tiết đơn dịch vụ cho 10 đơn trên (mỗi đơn 2 chi tiết)
INSERT INTO ChiTietDonDichVu (idCTDH, idDonDichVu, idLinhKien, idLoi, loaiDichVu, moTa, soLuong, ngayKetThucBH, thoiGianThemLinhKien, hanBaoHanh)
VALUES
('CT001', 'DDV001', NULL, 'L021', N'Sửa chữa', N'Sửa lỗi không lên hình', 1, '2025-05-04', '2025-01-02 10:00:00.000', 0),
('CT002', 'DDV001', 'LK001', NULL, N'Lắp đặt', N'Thay tụ điện mới', 1, NULL, '2025-01-02 10:00:00.000', 1),
('CT003', 'DDV002', NULL, 'L025', N'Sửa chữa', N'Khắc phục lỗi không lên màn hình', 1, '2025-05-04', '2025-01-02 10:00:00.000', 0),
('CT004', 'DDV002', 'LK002', NULL, N'Lắp đặt', N'Thay điện trở công suất', 2, NULL, '2025-01-02 10:00:00.000', 1),
('CT005', 'DDV003', 'LK016', NULL, N'Lắp đặt', N'Thay jack nguồn cho màn hình', 1, NULL, '2025-01-02 10:00:00.000', 1),
('CT006', 'DDV003', NULL, 'L028', N'Sửa chữa', N'Cài đặt driver màn hình', 1, '2025-05-04', '2025-01-02 10:00:00.000', 0),
('CT007', 'DDV004', NULL, 'L030', N'Sửa chữa', N'Sửa lỗi liệt phím', 1, '2025-05-04', '2025-01-02 10:00:00.000', 0),
('CT008', 'DDV004', 'LK004', NULL, N'Lắp đặt', N'Thay diode bàn phím', 3, NULL, '2025-01-02 10:00:00.000', 1),
('CT009', 'DDV005', NULL, 'L032', N'Sửa chữa', N'Sửa lỗi không nhận chuột', 1, '2025-05-04', '2025-01-02 10:00:00.000', 0),
('CT010', 'DDV005', 'LK005', NULL, N'Lắp đặt', N'Thay triac cảm biến', 1, NULL, '2025-01-02 10:00:00.000', 1),
('CT011', 'DDV006', NULL, 'L034', N'Sửa chữa', N'Sửa lỗi không phát âm thanh', 1, '2025-05-06', '2025-01-02 10:00:00.000', 0),
('CT012', 'DDV006', 'LK006', NULL, N'Lắp đặt', N'Thay MOSFET khuếch đại', 1, NULL, '2025-01-02 10:00:00.000', 1),
('CT013', 'DDV007', NULL, 'L036', N'Sửa chữa', N'Sửa lỗi không kết nối Bluetooth', 1, '2025-05-06', '2025-01-02 10:00:00.000', 0),
('CT014', 'DDV007', 'LK007', NULL, N'Lắp đặt', N'Thay IC nguồn', 1, NULL, '2025-01-02 10:00:00.000', 1),
('CT015', 'DDV008', 'LK008', NULL, N'Lắp đặt', N'Lắp đặt SSD mới', 1, NULL, '2025-01-02 10:00:00.000', 1),
('CT016', 'DDV008', NULL, 'L038', N'Sửa chữa', N'Cài đặt hệ điều hành', 1, '2025-05-09', '2025-01-02 10:00:00.000', 0),
('CT017', 'DDV009', NULL, 'L039', N'Sửa chữa', N'Khôi phục dữ liệu ổ cứng', 1, '2025-05-09', '2025-01-02 10:00:00.000', 0),
('CT018', 'DDV009', 'LK009', NULL, N'Lắp đặt', N'Thay relay điều khiển', 1, NULL, '2025-01-02 10:00:00.000', 1),
('CT019', 'DDV010', 'LK010', NULL, N'Lắp đặt', N'Lắp đặt RAM mới', 1, NULL, '2025-01-02 10:00:00.000', 1),
('CT020', 'DDV010', NULL, 'L040', N'Sửa chữa', N'Kiểm tra tương thích RAM', 1, '2025-05-09', '2025-01-02 10:00:00.000', 0);


-- select * from [User]
-- select * from DonDichVu

INSERT INTO HinhAnh (idHinhAnh,idCTDH, anh, loaiHinhAnh)
VALUES 
('HA00001', 'CT001', 'bao_hanh_1.jpg', N'Bảo hành'),
('HA00002', 'CT002', 'thiet_bi_1.jpg', N'Thiết bị linh kiện'),
('HA00003', 'CT003', 'bao_hanh_2.jpg', N'Bảo hành'),
('HA00004', 'CT004', 'khach_hang_1.jpg', N'Thiêt bị Linh kiện'),
('HA00005', 'CT005', 'bao_hanh_3.jpg', N'Bảo hành'),
('HA00006', 'CT006', 'thiet_bi_2.jpg', N'Thiêt bị Linh kiện'),
('HA00007', 'CT007', 'linh_kien_2.jpg', N'Thiêt bị Linh kiện'),
('HA00008', 'CT008', 'bao_hanh_4.jpg', N'Bảo hành'),
('HA00009', 'CT009', 'bao_hanh_5.jpg', N'Bảo hành'),
('HA00010', 'CT010', 'thiet_bi_3.jpg', N'Thiêt bị Linh kiện');


INSERT INTO DanhGia (idDanhGia, idDonDichVu, danhGiaNhanVien, danhGiaDichVu, gopY)
VALUES 
('DG00001', 'DDV001', 5, 4, N'Nhân viên thân thiện, dịch vụ tốt'),
('DG00002', 'DDV002', 3, 3, N'Thời gian chờ hơi lâu'),
('DG00003', 'DDV003', 4, 5, N'Hài lòng với chất lượng'),
('DG00004', 'DDV004', 2, 2, N'Cần cải thiện dịch vụ'),
('DG00005', 'DDV005', 5, 5, N'Tuyệt vời!'),
('DG00006', 'DDV006', 1, 2, N'Không hài lòng'),
('DG00007', 'DDV007', 4, 4, N'Ổn, sẽ quay lại'),
('DG00008', 'DDV008', 3, 4, N'Nhân viên cần chuyên nghiệp hơn'),
('DG00009', 'DDV009', 5, 3, N'Dịch vụ tốt nhưng giá cao'),
('DG00010', 'DDV010', 4, 4, N'Tốt, đúng như mong đợi');

-- select * from HinhAnh
-- select * from DanhGia



-- KHANH
--========================================Trigger cập nhật trạng thái nhân viên=============================
CREATE TRIGGER trg_UpdateTrangThaiNhanVienKyThuat 
ON ChiTietDonDichVu
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Cập nhật trạng thái của các nhân viên kỹ thuật liên quan
    UPDATE u
    SET u.trangThai = 
        CASE 
            WHEN NOT EXISTS (
                SELECT 1
                FROM DonDichVu ddv
                WHERE ddv.idNhanVienKyThuat = u.idUser
                AND ddv.trangThaiDon = N'Hoàn thành'
            ) THEN 1  -- rảnh
            ELSE 0     -- bận
        END
    FROM [User] u
    WHERE u.idRole = 'R002' -- chỉ cập nhật nhân viên kỹ thuật
    AND u.idUser IN (
        SELECT DISTINCT ddv.idNhanVienKyThuat
        FROM inserted i
        INNER JOIN DonDichVu ddv ON i.idDonDichVu = ddv.idDonDichVu
        WHERE ddv.idNhanVienKyThuat IS NOT NULL
    );
END;
GO


--================================================Function tạo id lỗi kế tiếp=======================
CREATE FUNCTION dbo.fn_GenerateNextLoiID()
RETURNS CHAR(7)
AS
BEGIN
    DECLARE @MaxNum INT
    DECLARE @NextID CHAR(7)

    -- Lấy phần số lớn nhất sau chữ 'L'
    SELECT @MaxNum = MAX(CAST(SUBSTRING(idLoi, 2, 3) AS INT))
    FROM LoaiLoi
    WHERE ISNUMERIC(SUBSTRING(idLoi, 2, 3)) = 1

    -- Nếu chưa có bản ghi nào, bắt đầu từ 1
    IF @MaxNum IS NULL
        SET @MaxNum = 0

    -- Tạo ID mới, định dạng L + 3 chữ số
    SET @NextID = 'L' + RIGHT('000' + CAST(@MaxNum + 1 AS VARCHAR), 3)

    RETURN @NextID
END
go
--=========================================Procedure insert Lỗi====================
CREATE PROCEDURE sp_InsertLoaiLoi
    @moTaLoi NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewID CHAR(7)
    
    -- tạo id nối tiếp
    SET @NewID = dbo.fn_GenerateNextLoiID()
    
    -- 
    INSERT INTO LoaiLoi (idLoi, moTaLoi)
    VALUES (@NewID, @moTaLoi)
    
   
    SELECT @NewID AS NewLoiID
    
    RETURN 0
END
go
--=========================================Procedure update Lỗi====================
CREATE PROCEDURE sp_UpdateLoaiLoi
    @idLoi CHAR(7),
    @moTaLoi NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- kiểm tra bản ghi tồn tại
    IF NOT EXISTS (SELECT 1 FROM LoaiLoi WHERE idLoi = @idLoi)
    BEGIN
        RAISERROR('ID lỗi không tồn tại.', 16, 1)
        RETURN 1
    END
    
 
    UPDATE LoaiLoi
    SET moTaLoi = @moTaLoi
    WHERE idLoi = @idLoi
    

    SELECT @idLoi as UpdatedLoiID, @moTaLoi as moTaLoi
    
    RETURN 0
END


--drop function fn_GenerateNextLoiID
--drop proc sp_InsertLoaiLoi
--drop proc sp_UpdateLoaiLoi


EXEC sp_InsertLoaiLoi N'Màn hình laptop bị sọc dọc'
EXEC sp_UpdateLoaiLoi 'L062', N'Màn hình laptop bị sọc ngang'

--select * from LoaiLoi where idLoi = 'L062'


--=========================================Function tạo id linh kiện kế tiếp====================
CREATE FUNCTION dbo.fn_GenerateNextLinhKienID()
RETURNS CHAR(7)
AS
BEGIN
    DECLARE @MaxNum INT
    DECLARE @NextID CHAR(7)

    -- Lấy phần số lớn nhất sau chữ 'LK'
    SELECT @MaxNum = MAX(CAST(SUBSTRING(idLinhKien, 3, 4) AS INT))
    FROM LinhKien
    WHERE ISNUMERIC(SUBSTRING(idLinhKien, 3, 4)) = 1

    -- Nếu chưa có bản ghi nào, bắt đầu từ 1
    IF @MaxNum IS NULL
        SET @MaxNum = 0

    -- Tạo ID mới, định dạng LK + 3 chữ số
    SET @NextID = 'LK' + RIGHT('000' + CAST(@MaxNum + 1 AS VARCHAR), 3)

    RETURN @NextID
END
GO
--=========================================Procedure insert linh kiện====================
CREATE PROCEDURE sp_InsertLinhKien
    @idNSX CHAR(7),
    @idLoaiLinhKien CHAR(7),
    @tenLinhKien NVARCHAR(100),
    @gia DECIMAL(18,2),
    @soLuong INT,
    @anh NVARCHAR(100),
    @thoiGianBaoHanh DATE,
    @dieuKienBaoHanh NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NewID CHAR(7)
    
    -- Tạo ID mới
    SET @NewID = dbo.fn_GenerateNextLinhKienID()
    
    -- Thêm bản ghi mới
    INSERT INTO LinhKien (
        idLinhKien, 
        idNSX, 
        idLoaiLinhKien, 
        tenLinhKien, 
        gia, 
        soLuong, 
        anh, 
        thoiGianBaoHanh, 
        dieuKienBaoHanh
    )
    VALUES (
        @NewID,
        @idNSX,
        @idLoaiLinhKien,
        @tenLinhKien,
        @gia,
        @soLuong,
        @anh,
        @thoiGianBaoHanh,
        @dieuKienBaoHanh
    )
    
    -- Trả về ID mới tạo
    SELECT @NewID AS NewLinhKienID
    
    RETURN 0
END
GO
--=========================================Procedure update Linh kiện====================
CREATE PROCEDURE sp_UpdateLinhKien
    @idLinhKien CHAR(7),
    @idNSX CHAR(7) = NULL,
    @idLoaiLinhKien CHAR(7) = NULL,
    @tenLinhKien NVARCHAR(100) = NULL,
    @gia DECIMAL(18,2) = NULL,
    @soLuong INT = NULL,
    @anh NVARCHAR(100) = NULL,
    @thoiGianBaoHanh DATE = NULL,
    @dieuKienBaoHanh NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra bản ghi tồn tại
    IF NOT EXISTS (SELECT 1 FROM LinhKien WHERE idLinhKien = @idLinhKien)
    BEGIN
        RAISERROR('ID linh kiện không tồn tại.', 16, 1)
        RETURN 1
    END
    
    -- Cập nhật chỉ các trường được cung cấp
    UPDATE LinhKien
    SET 
        idNSX = CASE WHEN @idNSX IS NOT NULL THEN @idNSX ELSE idNSX END,
        idLoaiLinhKien = CASE WHEN @idLoaiLinhKien IS NOT NULL THEN @idLoaiLinhKien ELSE idLoaiLinhKien END,
        tenLinhKien = CASE WHEN @tenLinhKien IS NOT NULL THEN @tenLinhKien ELSE tenLinhKien END,
        gia = CASE WHEN @gia IS NOT NULL THEN @gia ELSE gia END,
        soLuong = CASE WHEN @soLuong IS NOT NULL THEN @soLuong ELSE soLuong END,
        anh = CASE WHEN @anh IS NOT NULL THEN @anh ELSE anh END,
        thoiGianBaoHanh = CASE WHEN @thoiGianBaoHanh IS NOT NULL THEN @thoiGianBaoHanh ELSE thoiGianBaoHanh END,
        dieuKienBaoHanh = CASE WHEN @dieuKienBaoHanh IS NOT NULL THEN @dieuKienBaoHanh ELSE dieuKienBaoHanh END
    WHERE idLinhKien = @idLinhKien
    
    -- Trả về thông tin đã cập nhật
    SELECT * FROM LinhKien WHERE idLinhKien = @idLinhKien
    
    RETURN 0
END
GO

EXEC sp_InsertLinhKien 
    @idNSX = 'NSX001',
    @idLoaiLinhKien = 'LLK001',
    @tenLinhKien = N'Tụ điện 500V 100uF',
    @gia = 35000,
    @soLuong = 200,
    @anh = 'tu_dien_500v.jpg',
    @thoiGianBaoHanh = '2025-12-31',
    @dieuKienBaoHanh = N'Bảo hành 5 tháng, không bảo hành khi bị phồng tụ'

select * from LinhKien

EXEC sp_UpdateLinhKien 
    @idLinhKien = 'LK021',
    @idNSX = 'NSX001',
    @idLoaiLinhKien = 'LLK001',
    @tenLinhKien = N'Tụ điện 550V 100uF (Phiên bản mới)',
    @gia = 28000,
    @soLuong = 350,
    @anh = 'tu_dien_550v_new.jpg'

select * from LinhKien where idLinhKien = 'LK021'


-- NHƯ
----------------- TRIGGER ------------------
-- I. Tự động cập nhật ngày hoàn thành đơn dịch vụ khi trạng thái đơn dịch vụ là "Hoàn thành"
CREATE TRIGGER tg_UpdateNgayHoanThanh
ON DonDichVu
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE ddv
    SET ngayHoanThanh = GETDATE()
    FROM DonDichVu ddv
    INNER JOIN inserted i ON ddv.idDonDichVu = i.idDonDichVu
    INNER JOIN deleted d ON ddv.idDonDichVu = d.idDonDichVu
    WHERE i.trangThaiDon = N'Hoàn thành'
      AND (d.trangThaiDon IS NULL OR d.trangThaiDon <> N'Hoàn thành');
END;

-- II. Tự động cập nhật thời gian chỉnh sửa mỗi khi có thay đổi trong bảng DonDichVu

CREATE TRIGGER trg_UpdateNgayChinhSua
ON DonDichVu
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE DonDichVu
    SET ngayChinhSua = GETDATE()
    FROM DonDichVu D
    INNER JOIN inserted I ON D.idDonDichVu = I.idDonDichVu;
END;
GO

-------------------- FUNCTION/ PROCEDURE ----------------------------------

--------------************************************-------------------------
-- I. Bảng DonDichVu
select * from DonDichVu
-- 1. Function tạo idDonDichVu tự động
CREATE FUNCTION fn_GetNextDonDichVuID()
RETURNS CHAR(6)
AS
BEGIN
    DECLARE @NextID CHAR(6);
    DECLARE @CurrentMaxID CHAR(6);

    SELECT @CurrentMaxID = MAX(idDonDichVu) FROM DonDichVu;

    IF @CurrentMaxID IS NULL
        SET @NextID = 'DDV001';
    ELSE
        SET @NextID = 'DDV' + RIGHT('000' + CAST(CAST(SUBSTRING(@CurrentMaxID, 4, 3) AS INT) + 1 AS VARCHAR), 3);

    RETURN @NextID;
END;
GO

--  2. Stored Procedure INSERT
CREATE PROCEDURE sp_InsertDonDichVu
    @idUser CHAR(7),
    @idKhachVangLai CHAR(7),
    @idNhanVienKyThuat CHAR(7),
    @idUserTaoDon CHAR(7),
    @idLoaiThietBi CHAR(7),
    @tenThietBi NVARCHAR(150),
    @loaiKhachHang NVARCHAR(50),
    @ngayTaoDon DATETIME,
	@ngayHoanThanh DATETIME,
    @tongTien DECIMAL(18,2),
    @hinhThucDichVu NVARCHAR(100),
    @loaiDonDichVu NVARCHAR(100),
    @phuongThucThanhToan NVARCHAR(100),
    @trangThaiDon NVARCHAR(150),
	@ngayChinhSua DATETIME
AS
BEGIN
    DECLARE @NewID CHAR(6);
    SET @NewID = dbo.fn_GetNextDonDichVuID();

    INSERT INTO DonDichVu
        (idDonDichVu, idUser, idKhachVangLai, idNhanVienKyThuat, idUserTaoDon, idLoaiThietBi, tenThietBi, loaiKhachHang, ngayTaoDon, ngayHoanThanh, tongTien, hinhThucDichVu, loaiDonDichVu, phuongThucThanhToan, trangThaiDon, ngayChinhSua)
    VALUES
        (@NewID, @idUser, @idKhachVangLai, @idNhanVienKyThuat, @idUserTaoDon, @idLoaiThietBi, @tenThietBi, @loaiKhachHang, @ngayTaoDon, @ngayHoanThanh, @tongTien, @hinhThucDichVu, @loaiDonDichVu, @phuongThucThanhToan, @trangThaiDon, @ngayChinhSua);

    PRINT N'Đơn dịch vụ đã được thêm thành công!';
END;
GO

-- 3. Stored Procedure UPDATE
CREATE PROCEDURE sp_UpdateDonDichVu
    @idDonDichVu CHAR(7),
    @idUser CHAR(7),
    @idKhachVangLai CHAR(7),
    @idNhanVienKyThuat CHAR(7),
    @idUserTaoDon CHAR(7),
    @idLoaiThietBi CHAR(7),
    @tenThietBi NVARCHAR(150),
    @loaiKhachHang NVARCHAR(50),
    @ngayTaoDon DATETIME,
	@ngayHoanThanh DATETIME,
    @tongTien DECIMAL(18,2),
    @hinhThucDichVu NVARCHAR(100),
    @loaiDonDichVu NVARCHAR(100),
    @phuongThucThanhToan NVARCHAR(100),
    @trangThaiDon NVARCHAR(150),
	@ngayChinhSua DATETIME
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM DonDichVu WHERE idDonDichVu = @idDonDichVu)
    BEGIN
        PRINT N'Mã đơn dịch vụ không tồn tại.';
        RETURN;
    END

    UPDATE DonDichVu
    SET
        idUser = @idUser,
        idKhachVangLai = @idKhachVangLai,
        idNhanVienKyThuat = @idNhanVienKyThuat,
        idUserTaoDon = @idUserTaoDon,
        idLoaiThietBi = @idLoaiThietBi,
        tenThietBi = @tenThietBi,
        loaiKhachHang = @loaiKhachHang,
        ngayTaoDon = @ngayTaoDon,
		ngayHoanThanh = @ngayHoanThanh,
        tongTien = @tongTien,
        hinhThucDichVu = @hinhThucDichVu,
        loaiDonDichVu = @loaiDonDichVu,
        phuongThucThanhToan = @phuongThucThanhToan,
        trangThaiDon = @trangThaiDon,
		ngayChinhSua = @ngayChinhSua
    WHERE idDonDichVu = @idDonDichVu;

    PRINT N'Đơn dịch vụ đã được cập nhật thành công!';
END;
GO

-- 4. Stored Procedure DELETE
CREATE PROCEDURE sp_DeleteDonDichVu
    @idDonDichVu CHAR(7)
AS
BEGIN
    DELETE FROM DonDichVu WHERE idDonDichVu = @idDonDichVu;
    PRINT N'Đơn dịch vụ đã được xóa thành công!';
END;
GO

--------------************************************-------------------------

-- Bảng ChiTietDonDichVu
select * from ChiTietDonDichVu
-- 1. Function tạo ID tự động
CREATE FUNCTION fn_GetNextChiTietDonDichVuID()
RETURNS CHAR(6)
AS
BEGIN
    DECLARE @NextID CHAR(6);
    DECLARE @CurrentMaxID CHAR(6);

    SELECT @CurrentMaxID = MAX(idCTDH) FROM ChiTietDonDichVu;

    IF @CurrentMaxID IS NULL
        SET @NextID = 'CT001';
    ELSE
        SET @NextID = 'CT' + RIGHT('000' + CAST(CAST(SUBSTRING(@CurrentMaxID, 3, 3) AS INT) + 1 AS VARCHAR), 3);

    RETURN @NextID;
END;
GO

--  2. Stored Procedure INSERT
CREATE PROCEDURE sp_InsertChiTietDonDichVu
    @idDonDichVu CHAR(7),
    @idLinhKien CHAR(7),
    @idLoi CHAR(7),
    @loaiDichVu NVARCHAR(50),
    @moTa NVARCHAR(MAX),
    @soLuong INT,
    @ngayKetThucBH DATETIME,
    @thoiGianThemLinhKien DATETIME,
    @hanBaoHanh INT
AS
BEGIN
    DECLARE @NewID CHAR(6);
    SET @NewID = dbo.fn_GetNextChiTietDonDichVuID();

    INSERT INTO ChiTietDonDichVu
        (idCTDH, idDonDichVu, idLinhKien, idLoi, loaiDichVu, moTa, soLuong, ngayKetThucBH, thoiGianThemLinhKien, hanBaoHanh)
    VALUES
        (@NewID, @idDonDichVu, @idLinhKien, @idLoi, @loaiDichVu, @moTa, @soLuong, @ngayKetThucBH, @thoiGianThemLinhKien, @hanBaoHanh);

    PRINT N'Chi tiết đơn dịch vụ đã được thêm thành công!';
END;
GO

-- 3. Stored Procedure UPDATE
CREATE PROCEDURE sp_UpdateChiTietDonDichVu
    @idCTDH CHAR(7),
    @idDonDichVu CHAR(7),
    @idLinhKien CHAR(7),
    @idLoi CHAR(7),
    @loaiDichVu NVARCHAR(50),
    @moTa NVARCHAR(MAX),
    @soLuong INT,
    @ngayKetThucBH DATETIME,
    @thoiGianThemLinhKien DATETIME,
    @hanBaoHanh INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM ChiTietDonDichVu WHERE idCTDH = @idCTDH)
    BEGIN
        PRINT N'Mã chi tiết đơn dịch vụ không tồn tại.';
        RETURN;
    END

    UPDATE ChiTietDonDichVu
    SET
        idDonDichVu = @idDonDichVu,
        idLinhKien = @idLinhKien,
        idLoi = @idLoi,
        loaiDichVu = @loaiDichVu,
        moTa = @moTa,
        soLuong = @soLuong,
        ngayKetThucBH = @ngayKetThucBH,
        thoiGianThemLinhKien = @thoiGianThemLinhKien,
        hanBaoHanh = @hanBaoHanh
    WHERE idCTDH = @idCTDH;

    PRINT N'Chi tiết đơn dịch vụ đã được cập nhật thành công!';
END;
GO

-- 4. Stored Procedure DELETE
CREATE PROCEDURE sp_DeleteChiTietDonDichVu
    @idCTDH CHAR(6)
AS
BEGIN
    DELETE FROM ChiTietDonDichVu WHERE idCTDH = @idCTDH;
    PRINT N'Chi tiết đơn dịch vụ đã được xóa thành công!';
END;
GO

--------------************************************-------------------------
-- Lộc 
CREATE TRIGGER trg_CapNhatSoLuongLinhKien
ON DonDichVu
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Chỉ thực hiện khi trạng thái đơn chuyển sang 'Hoàn thành'
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.idDonDichVu = d.idDonDichVu
        WHERE i.trangThaiDon = N'Hoàn thành' AND d.trangThaiDon != N'Hoàn thành'
    )
    BEGIN
        PRINT N'Trigger đang cập nhật số lượng linh kiện...';

        -- Cập nhật số lượng linh kiện (chỉ khi có idLinhKien)
        UPDATE lk
        SET lk.soLuong = lk.soLuong - ct.soLuong
        FROM LinhKien lk
        JOIN ChiTietDonDichVu ct ON lk.idLinhKien = ct.idLinhKien
        JOIN inserted i ON ct.idDonDichVu = i.idDonDichVu
        WHERE ct.idLinhKien IS NOT NULL;

        PRINT N'Đã cập nhật xong.';
    END
    ELSE
    BEGIN
        PRINT N'Không có thay đổi trạng thái sang "Hoàn thành". Trigger không thực hiện.';
    END
END;


-- test trigger 
UPDATE DonDichVu
SET trangThaiDon = N'Chưa hoàn thành'
WHERE idDonDichVu = 'DDV001';

UPDATE DonDichVu
SET trangThaiDon = N'Hoàn thành'
WHERE idDonDichVu = 'DDV001';

select * from LinhKien

--------------************************************-------------------------
--function / proceduce

-- Nếu đã có sequence, xóa đi trước (nếu cần)
-- Xóa sequence cũ nếu có
IF EXISTS (SELECT * FROM sys.sequences WHERE name = 'seq_Role')
    DROP SEQUENCE dbo.seq_Role;
GO

-- Tìm ID lớn nhất hiện có trong bảng Role để khởi động lại sequence
DECLARE @nextRoleId INT = ISNULL(
    (SELECT MAX(CAST(SUBSTRING(idRole, 2, LEN(idRole)) AS INT)) FROM dbo.Role), 
    0
) + 1;

-- Tạo sequence mới bắt đầu từ ID tiếp theo
DECLARE @sql NVARCHAR(MAX) = 
    'CREATE SEQUENCE dbo.seq_Role AS INT START WITH ' 
    + CAST(@nextRoleId AS NVARCHAR) 
    + ' INCREMENT BY 1;';
EXEC(@sql);
GO


CREATE OR ALTER PROCEDURE dbo.sp_InsertRole
    @tenRole NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @n INT = NEXT VALUE FOR dbo.seq_Role;
    DECLARE @newId CHAR(4) = 'R' + RIGHT('000' + CAST(@n AS VARCHAR(3)), 3);

    INSERT INTO dbo.[Role](idRole, tenRole)
    VALUES(@newId, @tenRole);

    SELECT @newId AS NewRoleId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateRole
    @idRole  CHAR(4),
    @tenRole NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.[Role]
    SET tenRole = @tenRole
    WHERE idRole = @idRole;

    IF @@ROWCOUNT = 0
        PRINT N'Không tìm thấy Role ID: ' + @idRole;
    ELSE
        PRINT N'Cập nhật Role thành công: ' + @idRole;
END;
GO

-- Xóa sequence cũ nếu có
IF EXISTS (SELECT * FROM sys.sequences WHERE name = 'seq_User')
    DROP SEQUENCE dbo.seq_User;
GO

-- Tìm ID lớn nhất trong bảng User
DECLARE @nextUserId INT = ISNULL(
    (SELECT MAX(CAST(SUBSTRING(idUser, 3, LEN(idUser)) AS INT)) 
     FROM dbo.[User] 
     WHERE idUser LIKE 'AD%'), 
    0
) + 1;

-- Tạo sequence mới bắt đầu từ giá trị tiếp theo
DECLARE @sqlUser NVARCHAR(MAX) = 
    'CREATE SEQUENCE dbo.seq_User AS INT START WITH ' 
    + CAST(@nextUserId AS NVARCHAR) 
    + ' INCREMENT BY 1;';
EXEC(@sqlUser);
GO


CREATE OR ALTER PROCEDURE dbo.sp_InsertUser
    @idRole    CHAR(4),
    @idPhuong  CHAR(7),
    @tenUser   VARCHAR(50),
    @hoVaTen   NVARCHAR(100),
    @SDT       VARCHAR(15),
    @matKhau   VARCHAR(100),
    @diaChi    NVARCHAR(200) = NULL,
    @ngaySinh  DATE          = NULL,
    @trangThai BIT           = 1,
    @CCCD      VARCHAR(20)   = NULL,
    @gioiTinh  BIT           = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @n INT = NEXT VALUE FOR dbo.seq_User;
    DECLARE @newId CHAR(7) = 'AD' + RIGHT('00000' + CAST(@n AS VARCHAR(5)), 5);

    INSERT INTO dbo.[User](
        idUser, idRole, idPhuong, tenUser, hoVaTen, SDT, matKhau,
        diaChi, ngaySinh, trangThai, CCCD, gioiTinh
    )
    VALUES(
        @newId, @idRole, @idPhuong, @tenUser, @hoVaTen, @SDT, @matKhau,
        @diaChi, @ngaySinh, @trangThai, @CCCD, @gioiTinh
    );

    SELECT @newId AS NewUserId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateUser
    @idUser    CHAR(7),
    @idRole    CHAR(4),
    @idPhuong  CHAR(7),
    @tenUser   VARCHAR(50),
    @hoVaTen   NVARCHAR(100),
    @SDT       VARCHAR(15),
    @matKhau   VARCHAR(100),
    @diaChi    NVARCHAR(200) = NULL,
    @ngaySinh  DATE          = NULL,
    @trangThai BIT           = 1,
    @CCCD      VARCHAR(20)   = NULL,
    @gioiTinh  BIT           = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.[User]
    SET 
        idRole    = @idRole,
        idPhuong  = @idPhuong,
        tenUser   = @tenUser,
        hoVaTen   = @hoVaTen,
        SDT       = @SDT,
        matKhau   = @matKhau,
        diaChi    = @diaChi,
        ngaySinh  = @ngaySinh,
        trangThai = @trangThai,
        CCCD      = @CCCD,
        gioiTinh  = @gioiTinh
    WHERE idUser = @idUser;

    IF @@ROWCOUNT = 0
        PRINT N'Không tìm thấy User ID: ' + @idUser;
    ELSE
        PRINT N'Cập nhật User thành công: ' + @idUser;
END;
GO


EXEC dbo.sp_InsertRole @tenRole = 'Admin';


--------------************************************-------------------------


INSERT INTO DonDichVu (
    idDonDichVu, idUser, idKhachVangLai, idNhanVienKyThuat, idUserTaoDon,
    idLoaiThietBi, tenThietBi, loaiKhachHang, ngayTaoDon, ngayHoanThanh,
    tongTien, hinhThucDichVu, loaiDonDichVu, phuongThucThanhToan, trangThaiDon, ngayChinhSua
)
VALUES
('DDV013', NULL, 'KVL001', 'NVKT007', 'CSKH012', 'TB002', N'PC Asus Gaming', N'Khách vãng lai', '2025-02-02 10:00:00', '2025-02-02 12:45:00', 1850000, N'Trực tiếp', N'Sửa chữa', N'Chuyển khoản', N'Hoàn thành', '2025-02-02 12:45:00'),
('DDV014', 'KH00022', NULL, 'NVKT008', 'CSKH013', 'TB003', N'Màn hình LG 24inch', N'Khách hàng thường', '2025-02-03 08:30:00', '2025-02-03 10:15:00', 650000, N'Tại nhà', N'Lắp đặt', N'Tiền mặt', N'Hoàn thành', '2025-02-03 10:15:00'),
('DDV015', NULL, 'KVL002', 'NVKT009', 'CSKH014', 'TB004', N'Bàn phím cơ', N'Khách vãng lai', '2025-02-04 14:00:00', '2025-02-04 15:30:00', 350000, N'Trực tiếp', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-04 15:30:00'),
('DDV016', 'KH00023', NULL, 'NVKT010', 'CSKH015', 'TB005', N'Chuột không dây', N'Khách hàng thường', '2025-02-05 11:00:00', '2025-02-05 12:00:00', 250000, N'Tại nhà', N'Sửa chữa', N'Chuyển khoản', N'Hoàn thành', '2025-02-05 12:00:00'),
('DDV017', NULL, 'KVL003', 'NVKT006', 'CSKH011', 'TB006', N'Loa vi tính', N'Khách vãng lai', '2025-02-06 13:30:00', '2025-02-06 15:00:00', 500000, N'Trực tiếp', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-06 15:00:00'),
('DDV018', 'KH00024', NULL, 'NVKT007', 'CSKH012', 'TB007', N'Tai nghe Bluetooth', N'Khách hàng thường', '2025-02-07 09:15:00', '2025-02-07 10:45:00', 450000, N'Tại nhà', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-07 10:45:00'),
('DDV019', NULL, 'KVL004', 'NVKT006', 'CSKH013', 'TB008', N'Ổ cứng SSD 512GB', N'Khách vãng lai', '2025-02-08 10:30:00', '2025-02-08 12:00:00', 1200000, N'Trực tiếp', N'Lắp đặt', N'Chuyển khoản', N'Hoàn thành', '2025-02-08 12:00:00'),
('DDV020', 'KH00025', NULL, 'NVKT008', 'CSKH014', 'TB009', N'Ổ cứng HDD 1TB', N'Khách hàng thường', '2025-02-09 14:00:00', '2025-02-09 15:30:00', 800000, N'Tại nhà', N'Sửa chữa', N'Tiền mặt', N'Hoàn thành', '2025-02-09 15:30:00'),
('DDV021', NULL, 'KVL005', 'NVKT010', 'CSKH015', 'TB010', N'RAM DDR4 8GB', N'Khách vãng lai', '2025-02-10 11:00:00', '2025-02-10 12:30:00', 600000, N'Trực tiếp', N'Lắp đặt', N'Chuyển khoản', N'Hoàn thành', '2025-02-10 12:30:00'),
('DDV022', NULL, 'KVL005', 'NVKT010', 'CSKH015', 'TB010', N'RAM DDR4 8GB', N'Khách vãng lai', '2025-02-10 11:00:00', '2025-02-10 12:30:00', 600000, N'Trực tiếp', N'Lắp đặt', N'Chuyển khoản', N'Đang sửa chữa', '2025-02-10 12:30:00');
