
using Microsoft.Data.SqlClient;

namespace CafePOS;

internal static class Database
{
    public static string ConnectionString = @"Server=LAPTOP-D2IAGK3D\SQLEXPRESS;Database=CafePOSDB;Integrated Security=True;TrustServerCertificate=True;";

    public static SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}
