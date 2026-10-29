using Microsoft.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL;

public class CustomerDAL
{
    public CustomerDTO? FindByCCCD(string cccd)
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("SELECT MaKH,HoTen,CCCD,ISNULL(SoDienThoai,'') FROM KhachHang WHERE CCCD=@CCCD", cn);
        cmd.Parameters.Add("@CCCD", System.Data.SqlDbType.VarChar, 20).Value = cccd;
        using var rd = cmd.ExecuteReader();
        if (!rd.Read()) return null;
        return new CustomerDTO { MaKH=rd.GetInt32(0), HoTen=rd.GetString(1), CCCD=rd.GetString(2), SoDienThoai=rd.GetString(3) };
    }

    public int Add(CustomerDTO c)
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand(@"INSERT INTO KhachHang(HoTen,CCCD,SoDienThoai)
OUTPUT INSERTED.MaKH VALUES(@HoTen,@CCCD,@SDT)", cn);
        cmd.Parameters.Add("@HoTen", System.Data.SqlDbType.NVarChar, 100).Value = c.HoTen;
        cmd.Parameters.Add("@CCCD", System.Data.SqlDbType.VarChar, 20).Value = c.CCCD;
        cmd.Parameters.Add("@SDT", System.Data.SqlDbType.VarChar, 15).Value = string.IsNullOrWhiteSpace(c.SoDienThoai) ? DBNull.Value : c.SoDienThoai;
        return (int)cmd.ExecuteScalar()!;
    }
}
