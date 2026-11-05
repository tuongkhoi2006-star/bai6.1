/* ============================================================
   RETAIL SALES SYSTEM - SalesDB
   Chay toan bo file trong SQL Server Management Studio (SSMS).
   ============================================================ */
IF DB_ID(N'SalesDB') IS NULL
    CREATE DATABASE SalesDB;
GO
USE SalesDB;
GO

IF OBJECT_ID(N'dbo.NhanVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhanVien
    (
        MaNV INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
        TenNV NVARCHAR(100) NOT NULL,
        SoDienThoai VARCHAR(15) NULL,
        ChucVu NVARCHAR(50) NULL,
    );
END;
GO

IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang
    (
        MaKH INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KhachHang PRIMARY KEY,
        TenKH NVARCHAR(100) NOT NULL,
        DienThoai VARCHAR(15) NULL,
        DiemTichLuy INT NOT NULL CONSTRAINT DF_KhachHang_Diem DEFAULT (0),
        CONSTRAINT CK_KhachHang_Diem CHECK (DiemTichLuy >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.SanPham', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SanPham
    (
        MaSP VARCHAR(20) NOT NULL CONSTRAINT PK_SanPham PRIMARY KEY,
        TenSP NVARCHAR(150) NOT NULL,
        GiaBan DECIMAL(18,2) NOT NULL,
        SoLuongTon INT NOT NULL,
        CONSTRAINT CK_SanPham_GiaBan CHECK (GiaBan > 0),
        CONSTRAINT CK_SanPham_SoLuongTon CHECK (SoLuongTon >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon
    (
        MaHD INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HoaDon PRIMARY KEY,
        NgayLap DATETIME NOT NULL CONSTRAINT DF_HoaDon_NgayLap DEFAULT (GETDATE()),
        MaNV INT NOT NULL,
        MaKH INT NULL,
        TongTien DECIMAL(18,2) NOT NULL CONSTRAINT DF_HoaDon_TongTien DEFAULT (0),
        CONSTRAINT FK_HoaDon_NhanVien FOREIGN KEY (MaNV) REFERENCES dbo.NhanVien(MaNV),
        CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH),
        CONSTRAINT CK_HoaDon_TongTien CHECK (TongTien >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.ChiTietHoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ChiTietHoaDon
    (
        MaHD INT NOT NULL,
        MaSP VARCHAR(20) NOT NULL,
        SoLuong INT NOT NULL,
        DonGia DECIMAL(18,2) NOT NULL,
        CONSTRAINT PK_ChiTietHoaDon PRIMARY KEY (MaHD, MaSP),
        CONSTRAINT FK_CTHD_HoaDon FOREIGN KEY (MaHD) REFERENCES dbo.HoaDon(MaHD),
        CONSTRAINT FK_CTHD_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP),
        CONSTRAINT CK_CTHD_SoLuong CHECK (SoLuong > 0),
        CONSTRAINT CK_CTHD_DonGia CHECK (DonGia > 0)
    );
END;
GO

-- Du lieu mau de co the mo ung dung va thu ngay.
IF NOT EXISTS (SELECT 1 FROM dbo.NhanVien)
BEGIN
    INSERT INTO dbo.NhanVien(TenNV, SoDienThoai, ChucVu) VALUES
    (N'Nguyễn Văn An', '0901000001', N'Thu ngân'),
    (N'Trần Thị Bình', '0901000002', N'Quản lý');
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.KhachHang)
BEGIN
    INSERT INTO dbo.KhachHang(TenKH, DienThoai, DiemTichLuy) VALUES
    (N'Khách hàng Nguyễn Minh', '0912000001', 10),
    (N'Khách hàng Trần Lan', '0912000002', 5);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SanPham)
BEGIN
    INSERT INTO dbo.SanPham(MaSP, TenSP, GiaBan, SoLuongTon) VALUES
    ('SP001', N'Nước suối 500ml', 6000, 200),
    ('SP002', N'Sữa tươi hộp 1L', 32000, 80),
    ('SP003', N'Mì ăn liền', 4500, 300),
    ('SP004', N'Bánh quy hộp', 25000, 60),
    ('SP005', N'Cà phê lon', 12000, 120),
    ('SP006', N'Dầu ăn 1L', 52000, 40),
    ('SP007', N'Khăn giấy', 18000, 70);
END;
GO

-- Unique filtered indexes: cho phep nhieu so dien thoai NULL, nhung so da nhap phai duy nhat.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_NhanVien_SoDienThoai' AND object_id = OBJECT_ID(N'dbo.NhanVien'))
    CREATE UNIQUE INDEX UX_NhanVien_SoDienThoai ON dbo.NhanVien(SoDienThoai) WHERE SoDienThoai IS NOT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_KhachHang_DienThoai' AND object_id = OBJECT_ID(N'dbo.KhachHang'))
    CREATE UNIQUE INDEX UX_KhachHang_DienThoai ON dbo.KhachHang(DienThoai) WHERE DienThoai IS NOT NULL;
GO

-- Index ho tro loc hoa don theo ngay va thong ke top san pham.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_HoaDon_NgayLap' AND object_id = OBJECT_ID(N'dbo.HoaDon'))
    CREATE INDEX IX_HoaDon_NgayLap ON dbo.HoaDon(NgayLap);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ChiTietHoaDon_MaSP' AND object_id = OBJECT_ID(N'dbo.ChiTietHoaDon'))
    CREATE INDEX IX_ChiTietHoaDon_MaSP ON dbo.ChiTietHoaDon(MaSP);
GO

PRINT N'Da tao / kiem tra xong CSDL SalesDB va 5 bang.';
GO
