using System.Data;
using Microsoft.Data.SqlClient;
using InventoryManagementWinForms.DTO;
namespace InventoryManagementWinForms.DAL;
public sealed class PhieuKhoDAL {
 public int Save(PhieuKhoDTO x) {
  if(x.ChiTiet.Count==0)throw new InvalidOperationException("Phiếu phải có ít nhất một dòng.");
  if(x.LoaiPhieu=="NHAP" && x.MaNCC==null)throw new InvalidOperationException("Phiếu nhập cần nhà cung cấp.");
  using var cn=Db.CreateConnection();cn.Open();using SqlTransaction trans=cn.BeginTransaction(IsolationLevel.Serializable);
  try {
   decimal total=x.ChiTiet.Sum(i=>i.SoLuong*i.DonGia);
   using var h=new SqlCommand("INSERT INTO dbo.PhieuKho(LoaiPhieu,MaNCC,TongTien) VALUES(@Loai,@NCC,@Tong); SELECT CAST(SCOPE_IDENTITY() AS INT);",cn,trans);
   h.Parameters.Add("@Loai",SqlDbType.NVarChar,10).Value=x.LoaiPhieu;
   h.Parameters.Add("@NCC",SqlDbType.Int).Value=x.MaNCC.HasValue?x.MaNCC.Value:DBNull.Value;
   var tp=h.Parameters.Add("@Tong",SqlDbType.Decimal);tp.Precision=18;tp.Scale=2;tp.Value=total;
   int id=Convert.ToInt32(h.ExecuteScalar());
   foreach(var ct in x.ChiTiet) {
    using var d=new SqlCommand("INSERT INTO dbo.ChiTietPhieuKho(MaPhieu,MaVatTu,SoLuong,DonGia) VALUES(@P,@M,@S,@G)",cn,trans);
    d.Parameters.Add("@P",SqlDbType.Int).Value=id;d.Parameters.Add("@M",SqlDbType.VarChar,20).Value=ct.MaVatTu;
    var s=d.Parameters.Add("@S",SqlDbType.Decimal);s.Precision=18;s.Scale=2;s.Value=ct.SoLuong;
    var g=d.Parameters.Add("@G",SqlDbType.Decimal);g.Precision=18;g.Scale=2;g.Value=ct.DonGia;d.ExecuteNonQuery();
    string sql=x.LoaiPhieu=="NHAP"?"UPDATE dbo.VatTu SET SoLuongTon=SoLuongTon+@S WHERE MaVatTu=@M":"UPDATE dbo.VatTu SET SoLuongTon=SoLuongTon-@S WHERE MaVatTu=@M AND SoLuongTon>=@S";
    using var stock=new SqlCommand(sql,cn,trans);
    var sp=stock.Parameters.Add("@S",SqlDbType.Decimal);sp.Precision=18;sp.Scale=2;sp.Value=ct.SoLuong;
    stock.Parameters.Add("@M",SqlDbType.VarChar,20).Value=ct.MaVatTu;
    if(stock.ExecuteNonQuery()==0) {
     if(x.LoaiPhieu=="XUAT")throw new InvalidOperationException("Không đủ hàng trong kho hoặc mã vật tư không tồn tại: "+ct.MaVatTu);
     throw new InvalidOperationException("Mã vật tư không tồn tại: "+ct.MaVatTu);
    }
   }
   trans.Commit();return id;
  } catch { try{trans.Rollback();}catch{} throw; }
 }
 public DataTable LowStock(decimal threshold) {
  using var cn=Db.CreateConnection();using var cmd=new SqlCommand("SELECT MaVatTu,TenVatTu,DVT,SoLuongTon FROM dbo.VatTu WHERE SoLuongTon<@T ORDER BY SoLuongTon",cn);
  var p=cmd.Parameters.Add("@T",SqlDbType.Decimal);p.Precision=18;p.Scale=2;p.Value=threshold;
  using var da=new SqlDataAdapter(cmd);var dt=new DataTable();da.Fill(dt);return dt;
 }
 public decimal InventoryValue() {
  using var cn=Db.CreateConnection();
  using var cmd=new SqlCommand("SELECT COALESCE(SUM(v.SoLuongTon*COALESCE(p.DonGia,0)),0) FROM dbo.VatTu v OUTER APPLY (SELECT TOP 1 c.DonGia FROM dbo.ChiTietPhieuKho c JOIN dbo.PhieuKho h ON h.MaPhieu=c.MaPhieu WHERE c.MaVatTu=v.MaVatTu AND h.LoaiPhieu=N'NHAP' ORDER BY h.NgayLap DESC,h.MaPhieu DESC) p",cn);
  cn.Open();return Convert.ToDecimal(cmd.ExecuteScalar());
 }
}
