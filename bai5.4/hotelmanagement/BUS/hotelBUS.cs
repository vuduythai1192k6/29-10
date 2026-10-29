using System.Data;
using HotelManagement.DAL;
using HotelManagement.DTO;

namespace HotelManagement.BUS;

public class HotelBUS
{
    private readonly RoomDAL roomDal = new();
    private readonly CustomerDAL customerDal = new();
    private readonly ServiceDAL serviceDal = new();
    private readonly BookingDAL bookingDal = new();

    public DataTable GetRooms() => roomDal.GetRooms();
    public List<RoomDTO> GetAvailableRooms() => roomDal.GetAvailableRooms();
    public List<ServiceDTO> GetServices() => serviceDal.GetAll();
    public DataTable GetServiceTable() => serviceDal.GetServiceTable();
    public List<BookingDTO> GetActiveBookings() => bookingDal.GetActiveBookings();

    public CustomerDTO GetOrCreateCustomer(string name, string cccd, string phone)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Vui lòng nhập họ tên.");
        if (string.IsNullOrWhiteSpace(cccd)) throw new ArgumentException("Vui lòng nhập CCCD.");
        var found = customerDal.FindByCCCD(cccd.Trim());
        if (found != null) return found;
        var customer = new CustomerDTO { HoTen=name.Trim(), CCCD=cccd.Trim(), SoDienThoai=phone.Trim() };
        customer.MaKH = customerDal.Add(customer);
        return customer;
    }

    public void CheckIn(int maKH, string room) => bookingDal.CheckIn(maKH, room);
    public void AddService(int maPhieu, int maDV, int quantity) => bookingDal.AddService(maPhieu, maDV, quantity);
    public decimal CheckOut(int maPhieu) => bookingDal.CheckOut(maPhieu);
    public void CompleteCleaning(string soPhong)
    {
        roomDal.CompleteCleaning(soPhong);
    }
}
