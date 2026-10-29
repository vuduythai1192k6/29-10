using System.Data;
using InventoryManagementWinForms.DAL;
using InventoryManagementWinForms.DTO;
namespace InventoryManagementWinForms.BUS;
public sealed class VatTuBUS {
 private readonly VatTuDAL dal=new();
 public DataTable Search(string k)=>dal.Search(k);
 public void Save(VatTuDTO x,bool isNew) {
  if(string.IsNullOrWhiteSpace(x.MaVatTu)||string.IsNullOrWhiteSpace(x.TenVatTu))throw new ArgumentException("Mã và tên vật tư bắt buộc.");
  if(x.SoLuongTon<0)throw new ArgumentException("Tồn kho không được âm.");dal.Save(x,isNew);
 }
 public void Delete(string ma)=>dal.Delete(ma);
}
