using Microsoft.Data.SqlClient;

namespace HotelManagement.DAL;

public static class Db
{
   
    public static string ConnectionString =
        @"Server=LAPTOP-D2IAGK3D\SQLEXPRESS;Database=HotelDB;Integrated Security=True;TrustServerCertificate=True;";

    public static SqlConnection Open()
    {
        var cn = new SqlConnection(ConnectionString);
        cn.Open();
        return cn;
    }
}
