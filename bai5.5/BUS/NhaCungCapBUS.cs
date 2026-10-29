using System.Data;
using InventoryManagementWinForms.DAL;
using InventoryManagementWinForms.DTO;
namespace InventoryManagementWinForms.BUS;
public sealed class NhaCungCapBUS {
 private readonly NhaCungCapDAL dal=new();
 public DataTable GetAll()=>dal.GetAll();
 public void Save(NhaCungCapDTO x,bool isNew) {if(string.IsNullOrWhiteSpace(x.TenNCC))throw new ArgumentException("Tên nhà cung cấp bắt buộc.");dal.Save(x,isNew);}
 public void Delete(int ma)=>dal.Delete(ma);
}
