using System;
using System.Collections.Generic;
using Sales_Retail_System.DAL;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.BUS
{
    public class BanHangBUS
    {
        private readonly BanHangDAL dal = new BanHangDAL();
        public int ThanhToan(int maNV, int? maKH, IList<CartItemDTO> gioHang, out decimal tongTien)
        {
            if (maNV <= 0) throw new ArgumentException("Vui long chon nhan vien ban hang.");
            if (gioHang == null || gioHang.Count == 0) throw new ArgumentException("Vui long them san pham vao gio hang.");
            foreach (CartItemDTO item in gioHang)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.MaSP)) throw new ArgumentException("Co san pham khong hop le trong gio hang.");
                if (item.SoLuong <= 0) throw new ArgumentException("So luong ban phai lon hon 0.");
            }
            return dal.ThanhToan(maNV, maKH, gioHang, out tongTien);
        }
        public decimal DoanhThuNgay(DateTime ngay) { return dal.DoanhThuNgay(ngay); }
        public decimal DoanhThuThang(int thang, int nam) { return dal.DoanhThuThang(thang, nam); }
        public List<TopSanPhamDTO> GetTop5SanPham() { return dal.GetTop5SanPham(); }
    }
}
