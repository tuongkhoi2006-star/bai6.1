using System;
using System.Collections.Generic;
using Sales_Retail_System.DAL;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.BUS
{
    public class NhanVienBUS
    {
        private readonly NhanVienDAL dal = new NhanVienDAL();
        public List<NhanVienDTO> GetAll() { return dal.GetAll(); }
        public void Insert(NhanVienDTO n) { Validate(n); dal.Insert(n); }
        public void Update(NhanVienDTO n) { Validate(n); if (n.MaNV <= 0) throw new ArgumentException("Ma nhan vien khong hop le."); dal.Update(n); }
        public void Delete(int maNV) { if (maNV <= 0) throw new ArgumentException("Hay chon nhan vien."); dal.Delete(maNV); }
        private static void Validate(NhanVienDTO n)
        {
            if (n == null) throw new ArgumentNullException("n");
            if (string.IsNullOrWhiteSpace(n.TenNV) || n.TenNV.Trim().Length > 100) throw new ArgumentException("Ten nhan vien khong duoc rong va toi da 100 ky tu.");
            if (!string.IsNullOrWhiteSpace(n.SoDienThoai) && n.SoDienThoai.Trim().Length > 15) throw new ArgumentException("So dien thoai toi da 15 ky tu.");
            if (n.ChucVu != null && n.ChucVu.Trim().Length > 50) throw new ArgumentException("Chuc vu toi da 50 ky tu.");
            n.TenNV = n.TenNV.Trim(); if (n.SoDienThoai != null) n.SoDienThoai = n.SoDienThoai.Trim(); if (n.ChucVu != null) n.ChucVu = n.ChucVu.Trim();
        }
    }
}
