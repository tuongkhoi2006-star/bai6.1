# BÀI 6.1 - RETAIL SALES SYSTEM

Ứng dụng quản lý bán hàng siêu thị mini viết bằng **C# Windows Forms - .NET Framework 4.8 - SQL Server**. Giao diện được tạo hoàn toàn bằng C# code, không cần mở file Designer.

## 1. Thành phần trong project

```text
Sales_Retail_System/
├── DTO/                 # Đối tượng truyền dữ liệu
├── DAL/                 # Truy cập SQL Server, SqlCommand, SqlParameter
├── BUS/                 # Kiểm tra dữ liệu và xử lý nghiệp vụ
├── GUI/                 # Giao diện Windows Forms
├── App.config           # Connection string
├── Program.cs
└── Sales_Retail_System.csproj
```

## 2. Tạo cơ sở dữ liệu

1. Mở SQL Server Management Studio (SSMS), kết nối SQL Server.
2. Mở file `SalesDB.sql` ở thư mục ngoài cùng của gói ZIP.
3. Nhấn **Execute**. Script tạo CSDL `SalesDB`, 5 bảng và dữ liệu mẫu:
   - `NhanVien`
   - `KhachHang`
   - `SanPham`
   - `HoaDon`
   - `ChiTietHoaDon`

## 3. Cấu hình kết nối

Mở `App.config`, sửa `Data Source` theo SQL Server trên máy. Ví dụ:

```xml
<add name="SalesDB"
     connectionString="Data Source=.;Initial Catalog=SalesDB;Integrated Security=True;TrustServerCertificate=True"
     providerName="System.Data.SqlClient" />
```

Một số giá trị thường gặp:
- SQL Server mặc định: `Data Source=.`
- SQL Express: `Data Source=.\SQLEXPRESS`
- LocalDB: `Data Source=(localdb)\MSSQLLocalDB`

Nếu dùng SQL Login thay vì Windows Authentication, đổi chuỗi kết nối thành `User ID=...;Password=...` theo tài khoản SQL Server của bạn.

## 4. Mở và chạy

1. Cài Visual Studio có workload **.NET desktop development**.
2. Mở `Sales_Retail_System.sln`.
3. Chọn cấu hình Debug và nhấn **F5**.
4. Project yêu cầu .NET Framework 4.8 và Windows để chạy WinForms.

## 5. Chức năng đã cài đặt

### Quản lý danh mục (CRUD 3-Tier)
- Sản phẩm: xem, thêm, sửa, xóa và tìm theo mã/tên.
- Khách hàng: xem, thêm, sửa, xóa.
- Nhân viên: xem, thêm, sửa, xóa.
- Các lệnh SQL truyền dữ liệu thông qua `SqlParameter`; tìm kiếm dùng `LIKE @TuKhoa` với giá trị `%từ khóa%`.

### Bán hàng / lập hóa đơn
- Chọn nhân viên, khách hàng hoặc khách lẻ.
- Chọn sản phẩm và số lượng để thêm vào giỏ hàng.
- Tổng tiền tự động cập nhật.
- Khi thanh toán, `BanHangDAL.ThanhToan()` mở một `SqlTransaction` mức `Serializable`:
  1. Khóa/đọc giá và tồn kho bằng `UPDLOCK, HOLDLOCK`.
  2. Kiểm tra tồn kho; nếu thiếu hàng thì phát sinh lỗi và `Rollback()`.
  3. Trừ tồn kho có điều kiện `SoLuongTon >= @SoLuong`.
  4. Thêm `HoaDon` và các dòng `ChiTietHoaDon`.
  5. Cộng điểm cho khách hàng: `FLOOR(TongTien / 100000)` điểm, vì `DiemTichLuy` là số nguyên.
  6. `Commit()` nếu tất cả thao tác thành công.
- Giá bán được đọc lại từ CSDL trong transaction, không tin giá gửi từ giao diện.

### Báo cáo - thống kê
- Doanh thu ngày: dùng `ExecuteScalar()`.
- Doanh thu tháng: dùng `ExecuteScalar()`.
- Top 5 sản phẩm bán chạy: dùng `SqlDataReader`.

## 6. Kịch bản kiểm thử gợi ý

1. Chạy SQL script, mở ứng dụng; dữ liệu mẫu có sẵn.
2. Vào tab **Sản phẩm**, tìm `SP001` hoặc tìm `nước`.
3. Vào tab **Bán hàng**, chọn nhân viên, chọn khách hàng; thêm `SP001` với số lượng 2 rồi thanh toán.
4. Kiểm tra tồn kho `SP001` giảm 2 trong tab Sản phẩm.
5. Kiểm tra điểm tích lũy khách hàng tăng theo tổng tiền / 100.000 (làm tròn xuống).
6. Thêm một sản phẩm vào giỏ với số lượng lớn hơn tồn kho (hoặc cập nhật tồn kho nhỏ trong SSMS), thử thanh toán và kiểm tra không có hóa đơn mới, tồn kho không bị trừ dở dang.
7. Vào tab **Báo cáo**, xem doanh thu ngày/tháng và Top 5.

## 7. Lưu ý

- Do khóa ngoại, sản phẩm/khách hàng/nhân viên đã được dùng trong hóa đơn có thể không xóa được. Đây là hành vi bảo vệ lịch sử giao dịch; thông báo lỗi SQL sẽ hiển thị trên giao diện.
- Số điện thoại dùng filtered unique index: nhiều bản ghi có thể để trống (NULL), nhưng số điện thoại đã nhập phải duy nhất.
- Giá nhập ở giao diện chấp nhận dấu thập phân theo định dạng invariant (ví dụ `12500.50`) hoặc định dạng vùng `vi-VN`.
- Các tham số truy vấn đều được bind qua `SqlParameter`. Không nên ghép chuỗi đầu vào của người dùng trực tiếp vào câu SQL.
