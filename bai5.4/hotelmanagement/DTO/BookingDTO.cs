namespace HotelManagement.DTO;

public class BookingDTO
{
    public int MaPhieu { get; set; }
    public string SoPhong { get; set; } = "";
    public int MaKH { get; set; }
    public string TenKhach { get; set; } = "";
    public DateTime NgayNhanPhong { get; set; }
    public DateTime? NgayTraPhong { get; set; }
    public decimal TongTienThanhToan { get; set; }
    public string DisplayText => $"Phiếu {MaPhieu} - Phòng {SoPhong} - {TenKhach}";
}
