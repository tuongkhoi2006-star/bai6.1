using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.DAL
{
    public class KhachHangDAL
    {
        public List<KhachHangDTO> GetAll()
        {
            var list = new List<KhachHangDTO>();
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT MaKH,TenKH,DienThoai,DiemTichLuy FROM KhachHang ORDER BY TenKH", conn))
            {
                conn.Open(); using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new KhachHangDTO { MaKH = r.GetInt32(0), TenKH = r.GetString(1), DienThoai = r.IsDBNull(2) ? "" : r.GetString(2), DiemTichLuy = r.GetInt32(3) });
            }
            return list;
        }

        public void Insert(KhachHangDTO k)
        {
            const string sql = "INSERT INTO KhachHang(TenKH,DienThoai,DiemTichLuy) VALUES(@TenKH,@DienThoai,@Diem)";
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddParameters(cmd, k); conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        public void Update(KhachHangDTO k)
        {
            const string sql = "UPDATE KhachHang SET TenKH=@TenKH,DienThoai=@DienThoai,DiemTichLuy=@Diem WHERE MaKH=@MaKH";
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddParameters(cmd, k); cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = k.MaKH; conn.Open();
                if (cmd.ExecuteNonQuery() == 0) throw new System.InvalidOperationException("Khong tim thay khach hang can sua.");
            }
        }

        public void Delete(int maKH)
        {
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand("DELETE FROM KhachHang WHERE MaKH=@MaKH", conn))
            {
                cmd.Parameters.Add("@MaKH", SqlDbType.Int).Value = maKH; conn.Open();
                if (cmd.ExecuteNonQuery() == 0) throw new System.InvalidOperationException("Khong tim thay khach hang can xoa.");
            }
        }

        private static void AddParameters(SqlCommand cmd, KhachHangDTO k)
        {
            cmd.Parameters.Add("@TenKH", SqlDbType.NVarChar, 100).Value = k.TenKH;
            cmd.Parameters.Add("@DienThoai", SqlDbType.VarChar, 15).Value = string.IsNullOrWhiteSpace(k.DienThoai) ? (object)System.DBNull.Value : k.DienThoai;
            cmd.Parameters.Add("@Diem", SqlDbType.Int).Value = k.DiemTichLuy;
        }
    }
}
