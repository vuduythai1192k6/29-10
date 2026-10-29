using Microsoft.Data.SqlClient;
using System;

namespace InventoryManagementWinForms.DAL
{
    public static class Db
    {
        public static string ConnectionString =>
            Environment.GetEnvironmentVariable("INVENTORYDB_CONNECTION")
            ?? @"Server=LAPTOP-D2IAGK3D\SQLEXPRESS;Initial Catalog=InventoryDB;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);
    }
}