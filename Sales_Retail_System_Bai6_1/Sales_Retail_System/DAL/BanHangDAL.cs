using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.DAL
{
    public class BanHangDAL
    {
        public int ThanhToan(int maNV, int? maKH, IList<CartItemDTO> gioHang, out decimal tongTien)
        {
            if (gioHang == null || gioHang.Count == 0) throw new InvalidOperationException("Gio hang dang trong.");
            tongTien = 0m;
            using (SqlConnection conn = Db.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        // Gop cac dong trung ma san pham de khong tao chi tiet trung khoa.
                        var items = gioHang.GroupBy(x => x.MaSP)
                            .Select(g => new { MaSP = g.Key, SoLuong = g.Sum(x => x.SoLuong) }).ToList();
                        var verified = new List<Tuple<string, int, decimal>>();

                        foreach (var item in items)
                        {
                            decimal donGia;
                            int tonKho;
                            using (SqlCommand read = new SqlCommand(
                                "SELECT GiaBan,SoLuongTon FROM SanPham WITH (UPDLOCK,HOLDLOCK) WHERE MaSP=@MaSP", conn, trans))
                            {
                                read.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = item.MaSP;
                                using (SqlDataReader r = read.ExecuteReader())
                                {
                                    if (!r.Read()) throw new InvalidOperationException("Khong tim thay san pham: " + item.MaSP);
                                    donGia = r.GetDecimal(0); tonKho = r.GetInt32(1);
                                }
                            }
                            if (item.SoLuong <= 0) throw new InvalidOperationException("So luong ban phai lon hon 0.");
                            if (tonKho < item.SoLuong)
                                throw new InvalidOperationException("San pham " + item.MaSP + " khong du ton kho. Ton hien tai: " + tonKho);

                            using (SqlCommand updateStock = new SqlCommand(
                                "UPDATE SanPham SET SoLuongTon=SoLuongTon-@SoLuong WHERE MaSP=@MaSP AND SoLuongTon>=@SoLuong", conn, trans))
                            {
                                updateStock.Parameters.Add("@SoLuong", SqlDbType.Int).Value = item.SoLuong;
                                updateStock.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = item.MaSP;
                                if (updateStock.ExecuteNonQuery() != 1)
                                    throw new InvalidOperationException("Khong the tru ton kho cho san pham " + item.MaSP + ". Vui long thu lai.");
                            }
                            verified.Add(Tuple.Create(item.MaSP, item.SoLuong, donGia));
                            tongTien += donGia * item.SoLuong;
                        }

                        int maHD;
                        using (SqlCommand insertHeader = new SqlCommand(
                            "INSERT INTO HoaDon(NgayLap,MaNV,MaKH,TongTien) OUTPUT INSERTED.MaHD VALUES(GETDATE(),@MaNV,@MaKH,@TongTien)", conn, trans))
                        {
                            insertHeader.Parameters.Add("@MaNV", SqlDbType.Int).Value = maNV;
                            insertHeader.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKH.HasValue ? (object)maKH.Value : DBNull.Value;
                            SqlParameter total = insertHeader.Parameters.Add("@TongTien", SqlDbType.Decimal);
                            total.Precision = 18; total.Scale = 2; total.Value = tongTien;
                            maHD = Convert.ToInt32(insertHeader.ExecuteScalar());
                        }

                        foreach (var item in verified)
                        {
                            using (SqlCommand detail = new SqlCommand(
                                "INSERT INTO ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) VALUES(@MaHD,@MaSP,@SoLuong,@DonGia)", conn, trans))
                            {
                                detail.Parameters.Add("@MaHD", SqlDbType.Int).Value = maHD;
                                detail.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = item.Item1;
                                detail.Parameters.Add("@SoLuong", SqlDbType.Int).Value = item.Item2;
                                SqlParameter price = detail.Parameters.Add("@DonGia", SqlDbType.Decimal);
                                price.Precision = 18; price.Scale = 2; price.Value = item.Item3;
                                detail.ExecuteNonQuery();
                            }
                        }

                        if (maKH.HasValue)
                        {
                            using (SqlCommand points = new SqlCommand(
                                "UPDATE KhachHang SET DiemTichLuy=DiemTichLuy+CONVERT(int,FLOOR(@TongTien/100000.0)) WHERE MaKH=@MaKH", conn, trans))
                            {
                                SqlParameter total = points.Parameters.Add("@TongTien", SqlDbType.Decimal);
                                total.Precision = 18; total.Scale = 2; total.Value = tongTien;
                                points.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKH.Value;
                                if (points.ExecuteNonQuery() != 1) throw new InvalidOperationException("Khong tim thay khach hang duoc chon.");
                            }
                        }

                        trans.Commit();
                        return maHD;
                    }
                    catch
                    {
                        try { trans.Rollback(); } catch { /* giao dich co the da rollback boi SQL Server */ }
                        throw;
                    }
                }
            }
        }

        public decimal DoanhThuNgay(DateTime ngay)
        {
            const string sql = "SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE NgayLap>=@TuNgay AND NgayLap<@DenNgay";
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TuNgay", SqlDbType.DateTime).Value = ngay.Date;
                cmd.Parameters.Add("@DenNgay", SqlDbType.DateTime).Value = ngay.Date.AddDays(1);
                conn.Open(); return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public decimal DoanhThuThang(int thang, int nam)
        {
            DateTime from = new DateTime(nam, thang, 1);
            const string sql = "SELECT ISNULL(SUM(TongTien),0) FROM HoaDon WHERE NgayLap>=@TuNgay AND NgayLap<@DenNgay";
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TuNgay", SqlDbType.DateTime).Value = from;
                cmd.Parameters.Add("@DenNgay", SqlDbType.DateTime).Value = from.AddMonths(1);
                conn.Open(); return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public List<TopSanPhamDTO> GetTop5SanPham()
        {
            var list = new List<TopSanPhamDTO>();
            const string sql = @"SELECT TOP 5 sp.MaSP,sp.TenSP,SUM(ct.SoLuong) AS TongSoLuongBan,
                                        SUM(ct.SoLuong*ct.DonGia) AS DoanhThu
                                 FROM ChiTietHoaDon ct INNER JOIN SanPham sp ON ct.MaSP=sp.MaSP
                                 GROUP BY sp.MaSP,sp.TenSP
                                 ORDER BY SUM(ct.SoLuong) DESC, SUM(ct.SoLuong*ct.DonGia) DESC";
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open(); using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new TopSanPhamDTO { MaSP = r.GetString(0), TenSP = r.GetString(1), TongSoLuongBan = r.GetInt32(2), DoanhThu = r.GetDecimal(3) });
            }
            return list;
        }
    }
}
