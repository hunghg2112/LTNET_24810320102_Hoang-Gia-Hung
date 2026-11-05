CREATE DATABASE HRMDB;
GO
USE HRMDB;
GO

-- 1. Bảng Phòng ban
CREATE TABLE PhongBan (
    MaPB INT IDENTITY(1,1) PRIMARY KEY,
    TenPB NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255)
);

-- 2. Bảng Hồ sơ nhân viên
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATETIME NULL,
    CCCD VARCHAR(20) UNIQUE NOT NULL,
    MaPB INT FOREIGN KEY REFERENCES PhongBan(MaPB),
    LuongCoBan DECIMAL(18,2) NOT NULL CHECK (LuongCoBan > 0),
    HeSoLuong FLOAT DEFAULT 1.0 CHECK (HeSoLuong > 0)
);

-- 3. Bảng Bảng chấm công tháng
CREATE TABLE BangChamCong (
    MaCong INT IDENTITY(1,1) PRIMARY KEY,
    MaNV INT FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam VARCHAR(7) NOT NULL, -- Định dạng: "11/2026"
    SoNgayCong FLOAT DEFAULT 0 CHECK (SoNgayCong <= 31),
    SoGioOT FLOAT DEFAULT 0 CHECK (SoGioOT >= 0),
    CONSTRAINT UQ_ChamCong UNIQUE (MaNV, ThangNam)
);

-- 4. Bảng Bảng lương chi tiết
CREATE TABLE BangLuong (
    MaLuong INT IDENTITY(1,1),
    MaNV INT FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam VARCHAR(7) NOT NULL,
    ThucLinh DECIMAL(18,2) NOT NULL CHECK (ThucLinh >= 0),
    PRIMARY KEY (MaNV, ThangNam)
);

-- NẠP DỮ LIỆU MẪU (ĐÃ SỬA LỖI SYNTAX)
INSERT INTO PhongBan (TenPB, MoTa) VALUES
(N'Hành chính Nhân sự', N'Quản lý nhân sự và thủ tục hành chính'),
(N'Kỹ thuật - An ninh mạng', N'Quản trị hệ thống và bảo mật thông tin'),
(N'Kế toán', N'Quản lý tài chính và dòng tiền');

INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) VALUES
(N'Hoàng Gia Hưng', '2004-12-21', '035204001234', 2, 15000000, 1.5),
(N'Nguyễn Văn A', '1998-05-10', '001098005678', 1, 10000000, 1.2),
(N'Trần Thị B', '2000-08-15', '001200009999', 3, 12000000, 1.3);

-- Nạp chấm công mẫu tháng 11/2026
INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT) VALUES
(1, '11/2026', 22, 10),
(2, '11/2026', 20, 5),
(3, '11/2026', 21.5, 0);