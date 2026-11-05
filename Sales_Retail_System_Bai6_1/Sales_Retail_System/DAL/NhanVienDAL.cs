using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Sales_Retail_System.DTO;

namespace Sales_Retail_System.DAL
{
    public class NhanVienDAL
    {
        public List<NhanVienDTO> GetAll()
        {
            var list = new List<NhanVienDTO>();
            using (SqlConnection conn = Db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("SELECT MaNV,TenNV,SoDienThoai,ChucVu FROM NhanVien ORDER BY TenNV", conn))
            {
                conn.Open(); using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new NhanVienDTO { MaNV = r.GetInt32(0), TenNV = r.GetString(1), SoDienThoai = r.IsDBNull(2) ? "" : r.GetString(2), ChucVu = r.IsDBNull(3) ? "" : r.GetString(3) });
            }
            return list;
        }

        public void Insert(NhanVienDTO n)
        {
            const string sql = "INSERT INTO NhanVien(TenNV,SoDienThoai,ChucVu) VALUES(@TenNV,@SDT,@ChucVu)";
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddParameters(cmd, n); conn.Open(); cmd.ExecuteNonQuery();
            }
        }

        public void Update(NhanVienDTO n)
        {
            const string sql = "UPDATE NhanVien SET TenNV=@TenNV,SoDienThoai=@SDT,ChucVu=@ChucVu WHERE MaNV=@MaNV";
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                AddParameters(cmd, n); cmd.Parameters.Add("@MaNV", SqlDbType.Int).Value = n.MaNV; conn.Open();
                if (cmd.ExecuteNonQuery() == 0) throw new System.InvalidOperationException("Khong tim thay nhan vien can sua.");
            }
        }

        public void Delete(int maNV)
        {
            using (SqlConnection conn = Db.GetConnection()) using (SqlCommand cmd = new SqlCommand("DELETE FROM NhanVien WHERE MaNV=@MaNV", conn))
            {
                cmd.Parameters.Add("@MaNV", SqlDbType.Int).Value = maNV; conn.Open();
                if (cmd.ExecuteNonQuery() == 0) throw new System.InvalidOperationException("Khong tim thay nhan vien can xoa.");
            }
        }

        private static void AddParameters(SqlCommand cmd, NhanVienDTO n)
        {
            cmd.Parameters.Add("@TenNV", SqlDbType.NVarChar, 100).Value = n.TenNV;
            cmd.Parameters.Add("@SDT", SqlDbType.VarChar, 15).Value = string.IsNullOrWhiteSpace(n.SoDienThoai) ? (object)System.DBNull.Value : n.SoDienThoai;
            cmd.Parameters.Add("@ChucVu", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(n.ChucVu) ? (object)System.DBNull.Value : n.ChucVu;
        }
    }
}
