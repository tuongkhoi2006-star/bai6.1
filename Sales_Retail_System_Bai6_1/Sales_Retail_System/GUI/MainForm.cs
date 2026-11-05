using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Sales_Retail_System.BUS;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.GUI
{
    public class MainForm : Form
    {
        private readonly SanPhamBUS sanPhamBUS = new SanPhamBUS();
        private readonly KhachHangBUS khachHangBUS = new KhachHangBUS();
        private readonly NhanVienBUS nhanVienBUS = new NhanVienBUS();
        private readonly BanHangBUS banHangBUS = new BanHangBUS();

        private DataGridView dgvSanPham, dgvKhachHang, dgvNhanVien, dgvGioHang, dgvTop5;
        private TextBox txtMaSP, txtTenSP, txtGiaSP, txtTonSP, txtTimSP;
        private TextBox txtMaKH, txtTenKH, txtDienThoaiKH, txtDiemKH;
        private TextBox txtMaNV, txtTenNV, txtSDTNV, txtChucVuNV;
        private ComboBox cboNhanVien, cboKhachHang, cboSanPham;
        private NumericUpDown nudSoLuong;
        private Label lblTongTien, lblKetQuaDoanhThu;
        private DateTimePicker dtNgay, dtThang;
        private List<CartItemDTO> gioHang = new List<CartItemDTO>();
        private string selectedProductCode;

        public MainForm()
        {
            Text = "RETAIL SALES SYSTEM - Quản lý bán hàng siêu thị mini";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1050, 700);
            Size = new Size(1250, 820);
            Font = new Font("Segoe UI", 9F);
            BuildInterface();
            LoadAllData();
        }

        private void BuildInterface()
        {
            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
            tabs.TabPages.Add(BuildProductTab());
            tabs.TabPages.Add(BuildCustomerTab());
            tabs.TabPages.Add(BuildEmployeeTab());
            tabs.TabPages.Add(BuildSalesTab());
            tabs.TabPages.Add(BuildReportTab());
            Controls.Add(tabs);
        }

        private TabPage BuildProductTab()
        {
            var page = new TabPage("Sản phẩm");
            var top = NewFlowPanel(150);
            txtMaSP = NewTextBox(120); txtTenSP = NewTextBox(190); txtGiaSP = NewTextBox(110); txtTonSP = NewTextBox(90); txtTimSP = NewTextBox(160);
            AddField(top, "Mã sản phẩm", txtMaSP); AddField(top, "Tên sản phẩm", txtTenSP);
            AddField(top, "Giá bán", txtGiaSP); AddField(top, "Số lượng tồn", txtTonSP); AddField(top, "Tìm theo mã / tên", txtTimSP);
            top.Controls.Add(NewButton("Tìm kiếm", delegate { SafeRun(delegate { dgvSanPham.DataSource = sanPhamBUS.Search(txtTimSP.Text); }); }));
            top.Controls.Add(NewButton("Tải lại", delegate { SafeRun(LoadProducts); }));
            top.Controls.Add(NewButton("Thêm", AddProduct)); top.Controls.Add(NewButton("Sửa", UpdateProduct));
            top.Controls.Add(NewButton("Xóa", DeleteProduct)); top.Controls.Add(NewButton("Làm mới ô", ClearProductInputs));
            dgvSanPham = NewGrid(); dgvSanPham.SelectionChanged += ProductSelectionChanged;
            page.Controls.Add(dgvSanPham); page.Controls.Add(top);
            return page;
        }

        private TabPage BuildCustomerTab()
        {
            var page = new TabPage("Khách hàng");
            var top = NewFlowPanel(125);
            txtMaKH = NewTextBox(75); txtMaKH.ReadOnly = true;
            txtTenKH = NewTextBox(200); txtDienThoaiKH = NewTextBox(140); txtDiemKH = NewTextBox(90);
            AddField(top, "Mã KH", txtMaKH); AddField(top, "Tên khách hàng", txtTenKH);
            AddField(top, "Điện thoại", txtDienThoaiKH); AddField(top, "Điểm tích lũy", txtDiemKH);
            top.Controls.Add(NewButton("Thêm", AddCustomer)); top.Controls.Add(NewButton("Sửa", UpdateCustomer));
            top.Controls.Add(NewButton("Xóa", DeleteCustomer)); top.Controls.Add(NewButton("Tải lại", delegate { SafeRun(LoadCustomers); }));
            top.Controls.Add(NewButton("Làm mới ô", ClearCustomerInputs));
            dgvKhachHang = NewGrid(); dgvKhachHang.SelectionChanged += CustomerSelectionChanged;
            page.Controls.Add(dgvKhachHang); page.Controls.Add(top);
            return page;
        }

        private TabPage BuildEmployeeTab()
        {
            var page = new TabPage("Nhân viên");
            var top = NewFlowPanel(125);
            txtMaNV = NewTextBox(75); txtMaNV.ReadOnly = true;
            txtTenNV = NewTextBox(200); txtSDTNV = NewTextBox(140); txtChucVuNV = NewTextBox(140);
            AddField(top, "Mã NV", txtMaNV); AddField(top, "Tên nhân viên", txtTenNV);
            AddField(top, "Số điện thoại", txtSDTNV); AddField(top, "Chức vụ", txtChucVuNV);
            top.Controls.Add(NewButton("Thêm", AddEmployee)); top.Controls.Add(NewButton("Sửa", UpdateEmployee));
            top.Controls.Add(NewButton("Xóa", DeleteEmployee)); top.Controls.Add(NewButton("Tải lại", delegate { SafeRun(LoadEmployees); }));
            top.Controls.Add(NewButton("Làm mới ô", ClearEmployeeInputs));
            dgvNhanVien = NewGrid(); dgvNhanVien.SelectionChanged += EmployeeSelectionChanged;
            page.Controls.Add(dgvNhanVien); page.Controls.Add(top);
            return page;
        }

        private TabPage BuildSalesTab()
        {
            var page = new TabPage("Bán hàng / Hóa đơn");
            var top = NewFlowPanel(170);
            cboNhanVien = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            cboKhachHang = new ComboBox { Width = 190, DropDownStyle = ComboBoxStyle.DropDownList };
            cboSanPham = new ComboBox { Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            nudSoLuong = new NumericUpDown { Width = 90, Minimum = 1, Maximum = 100000, Value = 1, ThousandsSeparator = true };
            AddField(top, "Nhân viên bán hàng", cboNhanVien); AddField(top, "Khách hàng", cboKhachHang);
            AddField(top, "Sản phẩm", cboSanPham); AddField(top, "Số lượng", nudSoLuong);
            top.Controls.Add(NewButton("Thêm vào giỏ", AddToCart));
            top.Controls.Add(NewButton("Xóa dòng chọn", RemoveCartItem));
            top.Controls.Add(NewButton("Làm trống giỏ", ClearCart));
            dgvGioHang = NewGrid();
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 65, Padding = new Padding(12), FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            lblTongTien = new Label { AutoSize = true, Text = "Tổng tiền: 0 đ", Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = Color.DarkGreen, Margin = new Padding(10, 8, 35, 0) };
            bottom.Controls.Add(lblTongTien);
            bottom.Controls.Add(NewButton("THANH TOÁN", Checkout, 150, 38));
            var note = new Label { AutoSize = true, Text = "Điểm tích lũy: 1 điểm / 100.000đ (làm tròn xuống)", ForeColor = Color.DimGray, Margin = new Padding(20, 12, 0, 0) };
            bottom.Controls.Add(note);
            page.Controls.Add(dgvGioHang); page.Controls.Add(bottom); page.Controls.Add(top);
            return page;
        }

        private TabPage BuildReportTab()
        {
            var page = new TabPage("Báo cáo - Thống kê");
            var top = NewFlowPanel(120);
            dtNgay = new DateTimePicker { Width = 145, Format = DateTimePickerFormat.Short, Value = DateTime.Today };
            dtThang = new DateTimePicker { Width = 145, Format = DateTimePickerFormat.Custom, CustomFormat = "MM/yyyy", ShowUpDown = true, Value = DateTime.Today };
            AddField(top, "Chọn ngày", dtNgay);
            top.Controls.Add(NewButton("Doanh thu ngày", ShowDailyRevenue));
            AddField(top, "Chọn tháng", dtThang);
            top.Controls.Add(NewButton("Doanh thu tháng", ShowMonthlyRevenue));
            top.Controls.Add(NewButton("Tải Top 5", LoadTop5));
            lblKetQuaDoanhThu = new Label { AutoSize = true, Text = "Doanh thu: --", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.DarkBlue, Margin = new Padding(15, 12, 0, 0) };
            top.Controls.Add(lblKetQuaDoanhThu);
            dgvTop5 = NewGrid();
            page.Controls.Add(dgvTop5); page.Controls.Add(top);
            return page;
        }

        private static FlowLayoutPanel NewFlowPanel(int height)
        {
            return new FlowLayoutPanel { Dock = DockStyle.Top, Height = height, Padding = new Padding(8), AutoScroll = true, WrapContents = true, BackColor = Color.FromArgb(245, 247, 250) };
        }

        private static TextBox NewTextBox(int width) { return new TextBox { Width = width, Font = new Font("Segoe UI", 9F) }; }

        private static void AddField(FlowLayoutPanel parent, string caption, Control input)
        {
            var panel = new Panel { Width = input.Width + 12, Height = 58, Margin = new Padding(4) };
            var label = new Label { Text = caption, Dock = DockStyle.Top, Height = 22, AutoSize = false, ForeColor = Color.FromArgb(55, 65, 81) };
            input.Location = new Point(0, 25); input.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            panel.Controls.Add(input); panel.Controls.Add(label); parent.Controls.Add(panel);
        }

        private static Button NewButton(string text, Action action, int width = 90, int height = 32)
        {
            var b = new Button { Text = text, Width = width, Height = height, Margin = new Padding(5, 24, 5, 3), BackColor = Color.White, FlatStyle = FlatStyle.Standard };
            b.Click += delegate { action(); };
            return b;
        }

        private static DataGridView NewGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                RowHeadersVisible = false, BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D, AutoGenerateColumns = true
            };
        }

        private void SafeRun(Action action)
        {
            try { action(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void LoadAllData()
        {
            SafeRun(delegate { LoadProducts(); LoadCustomers(); LoadEmployees(); LoadSalesCombos(); LoadTop5(); });
        }

        private void LoadProducts() { dgvSanPham.DataSource = null; dgvSanPham.DataSource = sanPhamBUS.GetAll(); }
        private void LoadCustomers() { dgvKhachHang.DataSource = null; dgvKhachHang.DataSource = khachHangBUS.GetAll(); }
        private void LoadEmployees() { dgvNhanVien.DataSource = null; dgvNhanVien.DataSource = nhanVienBUS.GetAll(); }

        private void LoadSalesCombos()
        {
            var staff = nhanVienBUS.GetAll();
            cboNhanVien.DataSource = null; cboNhanVien.DataSource = staff; cboNhanVien.DisplayMember = "TenNV"; cboNhanVien.ValueMember = "MaNV";
            var customers = khachHangBUS.GetAll();
            customers.Insert(0, new KhachHangDTO { MaKH = 0, TenKH = "-- Khách lẻ --", DiemTichLuy = 0 });
            cboKhachHang.DataSource = null; cboKhachHang.DataSource = customers; cboKhachHang.DisplayMember = "TenKH"; cboKhachHang.ValueMember = "MaKH"; cboKhachHang.SelectedIndex = 0;
            var products = sanPhamBUS.GetAll();
            cboSanPham.DataSource = null; cboSanPham.DataSource = products; cboSanPham.DisplayMember = "TenSP"; cboSanPham.ValueMember = "MaSP";
        }

        private void ProductSelectionChanged(object sender, EventArgs e)
        {
            SanPhamDTO p = dgvSanPham.CurrentRow == null ? null : dgvSanPham.CurrentRow.DataBoundItem as SanPhamDTO;
            if (p == null) return;
            selectedProductCode = p.MaSP; txtMaSP.Text = p.MaSP; txtTenSP.Text = p.TenSP; txtGiaSP.Text = p.GiaBan.ToString("0.##", CultureInfo.InvariantCulture); txtTonSP.Text = p.SoLuongTon.ToString();
        }

        private void CustomerSelectionChanged(object sender, EventArgs e)
        {
            KhachHangDTO k = dgvKhachHang.CurrentRow == null ? null : dgvKhachHang.CurrentRow.DataBoundItem as KhachHangDTO;
            if (k == null) return;
            txtMaKH.Text = k.MaKH.ToString(); txtTenKH.Text = k.TenKH; txtDienThoaiKH.Text = k.DienThoai; txtDiemKH.Text = k.DiemTichLuy.ToString();
        }

        private void EmployeeSelectionChanged(object sender, EventArgs e)
        {
            NhanVienDTO n = dgvNhanVien.CurrentRow == null ? null : dgvNhanVien.CurrentRow.DataBoundItem as NhanVienDTO;
            if (n == null) return;
            txtMaNV.Text = n.MaNV.ToString(); txtTenNV.Text = n.TenNV; txtSDTNV.Text = n.SoDienThoai; txtChucVuNV.Text = n.ChucVu;
        }

        private void AddProduct()
        {
            SafeRun(delegate
            {
                sanPhamBUS.Insert(new SanPhamDTO { MaSP = txtMaSP.Text, TenSP = txtTenSP.Text, GiaBan = ParseDecimal(txtGiaSP.Text, "Giá bán"), SoLuongTon = ParseInt(txtTonSP.Text, "Số lượng tồn") });
                LoadProducts(); LoadSalesCombos(); ClearProductInputs(); MessageBox.Show("Đã thêm sản phẩm.");
            });
        }
        private void UpdateProduct()
        {
            SafeRun(delegate
            {
                string key = string.IsNullOrWhiteSpace(selectedProductCode) ? txtMaSP.Text : selectedProductCode;
                if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Hãy chọn sản phẩm cần sửa.");
                sanPhamBUS.Update(new SanPhamDTO { MaSP = key, TenSP = txtTenSP.Text, GiaBan = ParseDecimal(txtGiaSP.Text, "Giá bán"), SoLuongTon = ParseInt(txtTonSP.Text, "Số lượng tồn") });
                LoadProducts(); LoadSalesCombos(); MessageBox.Show("Đã cập nhật sản phẩm.");
            });
        }
        private void DeleteProduct()
        {
            SafeRun(delegate
            {
                string key = string.IsNullOrWhiteSpace(selectedProductCode) ? txtMaSP.Text : selectedProductCode;
                if (string.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("Hãy chọn sản phẩm cần xóa.");
                if (MessageBox.Show("Xóa sản phẩm " + key + "?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                sanPhamBUS.Delete(key); LoadProducts(); LoadSalesCombos(); ClearProductInputs();
            });
        }
        private void ClearProductInputs() { selectedProductCode = null; txtMaSP.Clear(); txtTenSP.Clear(); txtGiaSP.Clear(); txtTonSP.Clear(); }

        private void AddCustomer()
        {
            SafeRun(delegate
            {
                khachHangBUS.Insert(new KhachHangDTO { TenKH = txtTenKH.Text, DienThoai = txtDienThoaiKH.Text, DiemTichLuy = string.IsNullOrWhiteSpace(txtDiemKH.Text) ? 0 : ParseInt(txtDiemKH.Text, "Điểm tích lũy") });
                LoadCustomers(); LoadSalesCombos(); ClearCustomerInputs(); MessageBox.Show("Đã thêm khách hàng.");
            });
        }
        private void UpdateCustomer()
        {
            SafeRun(delegate
            {
                khachHangBUS.Update(new KhachHangDTO { MaKH = ParseInt(txtMaKH.Text, "Mã khách hàng"), TenKH = txtTenKH.Text, DienThoai = txtDienThoaiKH.Text, DiemTichLuy = ParseInt(txtDiemKH.Text, "Điểm tích lũy") });
                LoadCustomers(); LoadSalesCombos(); MessageBox.Show("Đã cập nhật khách hàng.");
            });
        }
        private void DeleteCustomer()
        {
            SafeRun(delegate
            {
                int id = ParseInt(txtMaKH.Text, "Mã khách hàng");
                if (MessageBox.Show("Xóa khách hàng mã " + id + "?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                khachHangBUS.Delete(id); LoadCustomers(); LoadSalesCombos(); ClearCustomerInputs();
            });
        }
        private void ClearCustomerInputs() { txtMaKH.Clear(); txtTenKH.Clear(); txtDienThoaiKH.Clear(); txtDiemKH.Text = "0"; }

        private void AddEmployee()
        {
            SafeRun(delegate
            {
                nhanVienBUS.Insert(new NhanVienDTO { TenNV = txtTenNV.Text, SoDienThoai = txtSDTNV.Text, ChucVu = txtChucVuNV.Text });
                LoadEmployees(); LoadSalesCombos(); ClearEmployeeInputs(); MessageBox.Show("Đã thêm nhân viên.");
            });
        }
        private void UpdateEmployee()
        {
            SafeRun(delegate
            {
                nhanVienBUS.Update(new NhanVienDTO { MaNV = ParseInt(txtMaNV.Text, "Mã nhân viên"), TenNV = txtTenNV.Text, SoDienThoai = txtSDTNV.Text, ChucVu = txtChucVuNV.Text });
                LoadEmployees(); LoadSalesCombos(); MessageBox.Show("Đã cập nhật nhân viên.");
            });
        }
        private void DeleteEmployee()
        {
            SafeRun(delegate
            {
                int id = ParseInt(txtMaNV.Text, "Mã nhân viên");
                if (MessageBox.Show("Xóa nhân viên mã " + id + "?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                nhanVienBUS.Delete(id); LoadEmployees(); LoadSalesCombos(); ClearEmployeeInputs();
            });
        }
        private void ClearEmployeeInputs() { txtMaNV.Clear(); txtTenNV.Clear(); txtSDTNV.Clear(); txtChucVuNV.Clear(); }

        private void AddToCart()
        {
            SafeRun(delegate
            {
                SanPhamDTO p = cboSanPham.SelectedItem as SanPhamDTO;
                if (p == null) throw new InvalidOperationException("Chưa có sản phẩm để bán.");
                int quantity = Decimal.ToInt32(nudSoLuong.Value);
                CartItemDTO existing = gioHang.FirstOrDefault(x => x.MaSP == p.MaSP);
                int desired = quantity + (existing == null ? 0 : existing.SoLuong);
                if (desired > p.SoLuongTon) throw new InvalidOperationException("Số lượng trong giỏ vượt quá tồn kho hiện tại (" + p.SoLuongTon + ").");
                if (existing == null) gioHang.Add(new CartItemDTO { MaSP = p.MaSP, TenSP = p.TenSP, SoLuong = quantity, DonGia = p.GiaBan });
                else existing.SoLuong = desired;
                BindCart();
            });
        }

        private void RemoveCartItem()
        {
            if (dgvGioHang.CurrentRow == null) return;
            CartItemDTO item = dgvGioHang.CurrentRow.DataBoundItem as CartItemDTO;
            if (item != null) { gioHang.RemoveAll(x => x.MaSP == item.MaSP); BindCart(); }
        }
        private void ClearCart() { gioHang.Clear(); BindCart(); }
        private void BindCart()
        {
            dgvGioHang.DataSource = null; dgvGioHang.DataSource = gioHang.ToList();
            lblTongTien.Text = "Tổng tiền: " + gioHang.Sum(x => x.ThanhTien).ToString("N0") + " đ";
        }

        private void Checkout()
        {
            SafeRun(delegate
            {
                NhanVienDTO staff = cboNhanVien.SelectedItem as NhanVienDTO;
                if (staff == null) throw new InvalidOperationException("Cần có nhân viên bán hàng trước khi thanh toán.");
                KhachHangDTO customer = cboKhachHang.SelectedItem as KhachHangDTO;
                int? maKH = customer == null || customer.MaKH == 0 ? (int?)null : customer.MaKH;
                if (gioHang.Count == 0) throw new InvalidOperationException("Giỏ hàng đang trống.");
                decimal preview = gioHang.Sum(x => x.ThanhTien);
                if (MessageBox.Show("Xác nhận thanh toán " + preview.ToString("N0") + " đ?", "Thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                decimal total;
                int maHD = banHangBUS.ThanhToan(staff.MaNV, maKH, gioHang, out total);
                MessageBox.Show("Thanh toán thành công!\nMã hóa đơn: " + maHD + "\nTổng tiền: " + total.ToString("N0") + " đ", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearCart(); LoadProducts(); LoadSalesCombos(); LoadTop5();
            });
        }

        private void ShowDailyRevenue()
        {
            SafeRun(delegate { decimal value = banHangBUS.DoanhThuNgay(dtNgay.Value.Date); lblKetQuaDoanhThu.Text = "Doanh thu ngày " + dtNgay.Value.ToString("dd/MM/yyyy") + ": " + value.ToString("N0") + " đ"; });
        }
        private void ShowMonthlyRevenue()
        {
            SafeRun(delegate { decimal value = banHangBUS.DoanhThuThang(dtThang.Value.Month, dtThang.Value.Year); lblKetQuaDoanhThu.Text = "Doanh thu tháng " + dtThang.Value.ToString("MM/yyyy") + ": " + value.ToString("N0") + " đ"; });
        }
        private void LoadTop5()
        {
            if (dgvTop5 == null) return;
            SafeRun(delegate { dgvTop5.DataSource = null; dgvTop5.DataSource = banHangBUS.GetTop5SanPham(); });
        }

        private static int ParseInt(string value, string field)
        {
            int result; if (!int.TryParse(value, out result)) throw new FormatException(field + " phải là số nguyên hợp lệ."); return result;
        }
        private static decimal ParseDecimal(string value, string field)
        {
            decimal result;
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) &&
                !decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out result))
                throw new FormatException(field + " phải là số hợp lệ. Ví dụ: 15000 hoặc 15.000,50 theo định dạng máy.");
            return result;
        }
    }
}
