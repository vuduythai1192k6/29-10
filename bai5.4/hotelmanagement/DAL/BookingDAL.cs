using Microsoft.Data.SqlClient;
using HotelManagement.DTO;

namespace HotelManagement.DAL;

public class BookingDAL
{
    public void CheckIn(int maKH, string soPhong)
    {
        using var cn = Db.Open();
        using var tx = cn.BeginTransaction();
        try
        {
            using (var check = new SqlCommand("SELECT TrangThai FROM Phong WITH (UPDLOCK,ROWLOCK) WHERE SoPhong=@p", cn, tx))
            {
                check.Parameters.AddWithValue("@p", soPhong);
                var status = check.ExecuteScalar()?.ToString();
                if (status != "Trống") throw new InvalidOperationException("Phòng không còn trống.");
            }
            using (var cmd = new SqlCommand("INSERT INTO PhieuDatPhong(SoPhong,MaKH,NgayNhanPhong) VALUES(@p,@k,GETDATE()); UPDATE Phong SET TrangThai=N'Đang ở' WHERE SoPhong=@p;", cn, tx))
            {
                cmd.Parameters.AddWithValue("@p", soPhong);
                cmd.Parameters.AddWithValue("@k", maKH);
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }
        catch { tx.Rollback(); throw; }
    }

    public List<BookingDTO> GetActiveBookings()
    {
        using var cn = Db.Open();
        using var cmd = new SqlCommand(@"SELECT pp.MaPhieu,pp.SoPhong,pp.MaKH,kh.HoTen,pp.NgayNhanPhong
FROM PhieuDatPhong pp JOIN KhachHang kh ON pp.MaKH=kh.MaKH
WHERE pp.NgayTraPhong IS NULL ORDER BY pp.NgayNhanPhong DESC", cn);
        using var rd = cmd.ExecuteReader();
        var list = new List<BookingDTO>();
        while (rd.Read())
            list.Add(new BookingDTO { MaPhieu=rd.GetInt32(0), SoPhong=rd.GetString(1), MaKH=rd.GetInt32(2), TenKhach=rd.GetString(3), NgayNhanPhong=rd.GetDateTime(4) });
        return list;
    }

    public void AddService(int maPhieu, int maDV, int soLuong)
    {
        if (soLuong < 1) throw new ArgumentException("Số lượng phải lớn hơn 0.");
        using var cn = Db.Open();
        using var cmd = new SqlCommand("INSERT INTO SuDungDichVu(MaPhieu,MaDV,SoLuong,ThoiGian) VALUES(@p,@d,@sl,GETDATE())", cn);
        cmd.Parameters.AddWithValue("@p", maPhieu);
        cmd.Parameters.AddWithValue("@d", maDV);
        cmd.Parameters.AddWithValue("@sl", soLuong);
        cmd.ExecuteNonQuery();
    }

    public decimal CheckOut(int maPhieu)
    {
        using var cn = Db.Open();
        using var tx = cn.BeginTransaction();
        try
        {
            decimal roomRate;
            DateTime checkIn;
            string room;
            using (var cmd = new SqlCommand(@"SELECT pp.SoPhong,pp.NgayNhanPhong,p.GiaTheoGio
FROM PhieuDatPhong pp JOIN Phong p ON pp.SoPhong=p.SoPhong
WHERE pp.MaPhieu=@id AND pp.NgayTraPhong IS NULL", cn, tx))
            {
                cmd.Parameters.AddWithValue("@id", maPhieu);
                using var rd = cmd.ExecuteReader();
                if (!rd.Read()) throw new InvalidOperationException("Không tìm thấy phiếu đang lưu trú.");
                room = rd.GetString(0); checkIn = rd.GetDateTime(1); roomRate = rd.GetDecimal(2);
            }
            var hours = Math.Max(1m, (decimal)(DateTime.Now - checkIn).TotalHours);
            decimal serviceTotal;
            using (var cmd = new SqlCommand(@"SELECT ISNULL(SUM(sd.SoLuong*dv.DonGia),0)
FROM SuDungDichVu sd JOIN DichVu dv ON sd.MaDV=dv.MaDV WHERE sd.MaPhieu=@id", cn, tx))
            {
                cmd.Parameters.AddWithValue("@id", maPhieu);
                serviceTotal = Convert.ToDecimal(cmd.ExecuteScalar());
            }
            decimal total = decimal.Ceiling(hours) * roomRate + serviceTotal;
            using (var cmd = new SqlCommand(@"UPDATE PhieuDatPhong SET NgayTraPhong=GETDATE(),TongTienThanhToan=@total WHERE MaPhieu=@id;
UPDATE Phong SET TrangThai=N'Đang dọn dẹp' WHERE SoPhong=@room;", cn, tx))
            {
                cmd.Parameters.AddWithValue("@total", total);
                cmd.Parameters.AddWithValue("@id", maPhieu);
                cmd.Parameters.AddWithValue("@room", room);
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
            return total;
        }
        catch { tx.Rollback(); throw; }
    }
}
