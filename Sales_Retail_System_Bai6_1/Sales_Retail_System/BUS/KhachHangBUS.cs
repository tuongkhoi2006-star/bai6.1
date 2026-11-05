using System;
using System.Collections.Generic;
using Sales_Retail_System.DAL;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.BUS
{
    public class KhachHangBUS
    {
        private readonly KhachHangDAL dal = new KhachHangDAL();
        public List<KhachHangDTO> GetAll() { return dal.GetAll(); }
        public void Insert(KhachHangDTO k) { Validate(k); dal.Insert(k); }
        public void Update(KhachHangDTO k) { Validate(k); if (k.MaKH <= 0) throw new ArgumentException("Ma khach hang khong hop le."); dal.Update(k); }
        public void Delete(int maKH) { if (maKH <= 0) throw new ArgumentException("Hay chon khach hang."); dal.Delete(maKH); }
        private static void Validate(KhachHangDTO k)
        {
            if (k == null) throw new ArgumentNullException("k");
            if (string.IsNullOrWhiteSpace(k.TenKH) || k.TenKH.Trim().Length > 100) throw new ArgumentException("Ten khach hang khong duoc rong va toi da 100 ky tu.");
            if (!string.IsNullOrWhiteSpace(k.DienThoai) && k.DienThoai.Trim().Length > 15) throw new ArgumentException("Dien thoai toi da 15 ky tu.");
            if (k.DiemTichLuy < 0) throw new ArgumentException("Diem tich luy khong duoc am.");
            k.TenKH = k.TenKH.Trim(); if (k.DienThoai != null) k.DienThoai = k.DienThoai.Trim();
        }
    }
}
