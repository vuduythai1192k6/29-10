using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL;

public class ServiceDAL
{
    public List<ServiceDTO> GetAll()
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("SELECT MaDV,TenDV,DonGia FROM DichVu ORDER BY TenDV", cn);
        using var rd = cmd.ExecuteReader();
        var list = new List<ServiceDTO>();
        while (rd.Read())
            list.Add(new ServiceDTO { MaDV=rd.GetInt32(0), TenDV=rd.GetString(1), DonGia=rd.GetDecimal(2) });
        return list;
    }

    public DataTable GetServiceTable()
    {
        using var cn = Db.Open();
        using var da = new SqlDataAdapter("SELECT MaDV,TenDV,DonGia FROM DichVu ORDER BY TenDV", cn);
        var dt = new DataTable(); da.Fill(dt); return dt;
    }
}
