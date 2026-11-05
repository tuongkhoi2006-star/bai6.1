using System;
using System.Collections.Generic;
using Sales_Retail_System.DAL;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.BUS
{
    public class SanPhamBUS
    {
        private readonly SanPhamDAL dal = new SanPhamDAL();
        public List<SanPhamDTO> GetAll() { return dal.GetAll(); }
        public List<SanPhamDTO> Search(string keyword) { return dal.Search(keyword); }
        public void Insert(SanPhamDTO p) { Validate(p); dal.Insert(p); }
        public void Update(SanPhamDTO p) { Validate(p); dal.Update(p); }
        public void Delete(string maSP) { if (string.IsNullOrWhiteSpace(maSP)) throw new ArgumentException("Hay chon ma san pham."); dal.Delete(maSP.Trim()); }
        private static void Validate(SanPhamDTO p)
        {
            if (p == null) throw new ArgumentNullException("p");
            if (string.IsNullOrWhiteSpace(p.MaSP) || p.MaSP.Trim().Length > 20) throw new ArgumentException("Ma san pham khong duoc rong va toi da 20 ky tu.");
            if (string.IsNullOrWhiteSpace(p.TenSP) || p.TenSP.Trim().Length > 150) throw new ArgumentException("Ten san pham khong duoc rong va toi da 150 ky tu.");
            if (p.GiaBan <= 0) throw new ArgumentException("Gia ban phai lon hon 0.");
            if (p.SoLuongTon < 0) throw new ArgumentException("So luong ton khong duoc am.");
            p.MaSP = p.MaSP.Trim(); p.TenSP = p.TenSP.Trim();
        }
    }
}
