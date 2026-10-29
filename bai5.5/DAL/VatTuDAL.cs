using System.Data;
using Microsoft.Data.SqlClient;
using InventoryManagementWinForms.DTO;
namespace InventoryManagementWinForms.DAL;
public sealed class VatTuDAL {
 public DataTable Search(string keyword) {
  using var cn=Db.CreateConnection();
  using var cmd=new SqlCommand("SELECT MaVatTu,TenVatTu,DVT,SoLuongTon FROM dbo.VatTu WHERE MaVatTu LIKE @K OR TenVatTu LIKE @K ORDER BY MaVatTu",cn);
  cmd.Parameters.Add("@K",SqlDbType.NVarChar,170).Value="%"+keyword.Trim()+"%";
  using var da=new SqlDataAdapter(cmd); var dt=new DataTable(); da.Fill(dt); return dt;
 }
 public void Save(VatTuDTO x,bool isNew) {
  using var cn=Db.CreateConnection(); cn.Open();
  using var cmd=new SqlCommand(isNew ?
   "INSERT INTO dbo.VatTu(MaVatTu,TenVatTu,DVT,SoLuongTon) VALUES(@Ma,@Ten,@DVT,@Ton)" :
   "UPDATE dbo.VatTu SET TenVatTu=@Ten,DVT=@DVT,SoLuongTon=@Ton WHERE MaVatTu=@Ma",cn);
  cmd.Parameters.Add("@Ma",SqlDbType.VarChar,20).Value=x.MaVatTu;
  cmd.Parameters.Add("@Ten",SqlDbType.NVarChar,150).Value=x.TenVatTu;
  cmd.Parameters.Add("@DVT",SqlDbType.NVarChar,20).Value=string.IsNullOrWhiteSpace(x.DVT)?DBNull.Value:x.DVT;
  var p=cmd.Parameters.Add("@Ton",SqlDbType.Decimal); p.Precision=18;p.Scale=2;p.Value=x.SoLuongTon;
  cmd.ExecuteNonQuery();
 }
 public void Delete(string ma) {
  using var cn=Db.CreateConnection(); using var cmd=new SqlCommand("DELETE FROM dbo.VatTu WHERE MaVatTu=@Ma",cn);
  cmd.Parameters.Add("@Ma",SqlDbType.VarChar,20).Value=ma; cn.Open();cmd.ExecuteNonQuery();
 }
}
