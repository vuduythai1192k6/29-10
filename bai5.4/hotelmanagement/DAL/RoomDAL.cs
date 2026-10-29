using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL;

public class RoomDAL
{
    public DataTable GetRooms()
    {
        using var cn = Db.Open();
        using var da = new SqlDataAdapter("SELECT SoPhong,LoaiPhong,GiaTheoGio,TrangThai FROM Phong ORDER BY SoPhong", cn);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    public List<RoomDTO> GetAvailableRooms()
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand("SELECT SoPhong,LoaiPhong,GiaTheoGio,TrangThai FROM Phong WHERE TrangThai=N'Trống' ORDER BY SoPhong", cn);
        using var rd = cmd.ExecuteReader();
        var list = new List<RoomDTO>();
        while (rd.Read())
            list.Add(new RoomDTO { SoPhong=rd.GetString(0), LoaiPhong=rd.GetString(1), GiaTheoGio=rd.GetDecimal(2), TrangThai=rd.GetString(3) });
        return list;
    }
    public void CompleteCleaning(string soPhong)
    {
        using var cn = Db.Open();

        using var cmd = new SqlCommand(
            @"UPDATE Phong
          SET TrangThai = N'Trống'
          WHERE SoPhong = @SoPhong
            AND TrangThai = N'Đang dọn dẹp'", cn);

        cmd.Parameters.AddWithValue("@SoPhong", soPhong);

        int rows = cmd.ExecuteNonQuery();

        if (rows == 0)
            throw new InvalidOperationException(
                "Phòng không tồn tại hoặc chưa ở trạng thái dọn dẹp.");
    }
}
