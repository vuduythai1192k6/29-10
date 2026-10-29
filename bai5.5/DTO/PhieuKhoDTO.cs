namespace InventoryManagementWinForms.DTO;
public sealed class PhieuKhoDTO {
 public string LoaiPhieu {get;set;} = "NHAP";
 public int? MaNCC {get;set;}
 public List<ChiTietPhieuDTO> ChiTiet {get;set;} = new();
}
