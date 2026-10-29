using System.Data;
using InventoryManagementWinForms.DAL;
using InventoryManagementWinForms.DTO;
namespace InventoryManagementWinForms.BUS;
public sealed class PhieuKhoBUS {
 private readonly PhieuKhoDAL dal=new();
 public int Save(PhieuKhoDTO x) {
  if(x.LoaiPhieu!="NHAP"&&x.LoaiPhieu!="XUAT")throw new ArgumentException("Loại phiếu không hợp lệ.");
  foreach(var c in x.ChiTiet){if(string.IsNullOrWhiteSpace(c.MaVatTu)||c.SoLuong<=0||c.DonGia<0)throw new ArgumentException("Mã vật tư, số lượng hoặc đơn giá không hợp lệ.");}
  if(x.ChiTiet.GroupBy(c=>c.MaVatTu,StringComparer.OrdinalIgnoreCase).Any(g=>g.Count()>1))throw new ArgumentException("Mỗi mã vật tư chỉ được có một dòng.");
  return dal.Save(x);
 }
 public DataTable LowStock(decimal t)=>dal.LowStock(t);
 public decimal InventoryValue()=>dal.InventoryValue();
}
