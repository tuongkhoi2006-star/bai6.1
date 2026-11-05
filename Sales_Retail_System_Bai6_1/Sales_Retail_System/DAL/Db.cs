using System.Configuration;
using System.Data.SqlClient;

namespace Sales_Retail_System.DAL
{
    internal static class Db
    {
        public static SqlConnection GetConnection()
        {
            ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings["SalesDB"];
            if (setting == null || string.IsNullOrWhiteSpace(setting.ConnectionString))
                throw new ConfigurationErrorsException("Chua cau hinh connection string SalesDB trong App.config.");
            return new SqlConnection(setting.ConnectionString);
        }
    }
}
