using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.DAL
{
    public class SanPhamDAL
    {
        public List<SanPhamDTO> GetAll()
        {
            var list = new List<SanPhamDTO>();
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham ORDER BY TenSP", conn))
            {
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(ReadProduct(r));
            }
            return list;
        }

        public List<SanPhamDTO> Search(string keyword)
        {
            var list = new List<SanPhamDTO>();
            const string sql = @"SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham
                                 WHERE MaSP LIKE @TuKhoa OR TenSP LIKE @TuKhoa ORDER BY TenSP";
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@TuKhoa", SqlDbType.NVarChar, 200).Value = "%" + (keyword ?? "").Trim() + "%";
                conn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(ReadProduct(r));
            }
            return list;
        }

        public void Insert(SanPhamDTO p)
        {
            const string sql = "INSERT INTO SanPham(MaSP,TenSP,GiaBan,SoLuongTon) VALUES(@MaSP,@TenSP,@GiaBan,@SoLuongTon)";
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddProductParameters(cmd, p, true);
                conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        public void Update(SanPhamDTO p)
        {
            const string sql = "UPDATE SanPham SET TenSP=@TenSP,GiaBan=@GiaBan,SoLuongTon=@SoLuongTon WHERE MaSP=@MaSP";
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddProductParameters(cmd, p, true);
                conn.Open();
                if (cmd.ExecuteNonQuery() == 0) throw new System.InvalidOperationException("Khong tim thay san pham can sua.");
            }
        }

        public void Delete(string maSP)
        {
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("DELETE FROM SanPham WHERE MaSP=@MaSP", conn))
            {
                cmd.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = maSP;
                conn.Open();
                if (cmd.ExecuteNonQuery() == 0) throw new System.InvalidOperationException("Khong tim thay san pham can xoa.");
            }
        }

        private static void AddProductParameters(SqlCommand cmd, SanPhamDTO p, bool includeId)
        {
            if (includeId) cmd.Parameters.Add("@MaSP", SqlDbType.VarChar, 20).Value = p.MaSP;
            cmd.Parameters.Add("@TenSP", SqlDbType.NVarChar, 150).Value = p.TenSP;
            var price = cmd.Parameters.Add("@GiaBan", SqlDbType.Decimal); price.Precision = 18; price.Scale = 2; price.Value = p.GiaBan;
            cmd.Parameters.Add("@SoLuongTon", SqlDbType.Int).Value = p.SoLuongTon;
        }

        private static SanPhamDTO ReadProduct(SqlDataReader r)
        {
            return new SanPhamDTO { MaSP = r.GetString(0), TenSP = r.GetString(1), GiaBan = r.GetDecimal(2), SoLuongTon = r.GetInt32(3) };
        }
    }
}
