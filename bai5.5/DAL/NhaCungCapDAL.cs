using System.Data;
using Microsoft.Data.SqlClient;
using InventoryManagementWinForms.DTO;
namespace InventoryManagementWinForms.DAL;
public sealed class NhaCungCapDAL {
 public DataTable GetAll() {
  using var cn=Db.CreateConnection(); using var cmd=new SqlCommand("SELECT MaNCC,TenNCC,DienThoai,DiaChi FROM dbo.NhaCungCap ORDER BY MaNCC DESC",cn);
  using var da=new SqlDataAdapter(cmd);var dt=new DataTable();da.Fill(dt);return dt;
 }
 public void Save(NhaCungCapDTO x,bool isNew) {
  using var cn=Db.CreateConnection();cn.Open();
  using var cmd=new SqlCommand(isNew?"INSERT INTO dbo.NhaCungCap(TenNCC,DienThoai,DiaChi) VALUES(@Ten,@SDT,@DC)":"UPDATE dbo.NhaCungCap SET TenNCC=@Ten,DienThoai=@SDT,DiaChi=@DC WHERE MaNCC=@Ma",cn);
  cmd.Parameters.Add("@Ten",SqlDbType.NVarChar,150).Value=x.TenNCC;
  cmd.Parameters.Add("@SDT",SqlDbType.VarChar,15).Value=string.IsNullOrWhiteSpace(x.DienThoai)?DBNull.Value:x.DienThoai;
  cmd.Parameters.Add("@DC",SqlDbType.NVarChar,255).Value=string.IsNullOrWhiteSpace(x.DiaChi)?DBNull.Value:x.DiaChi;
  if(!isNew)cmd.Parameters.Add("@Ma",SqlDbType.Int).Value=x.MaNCC;
  cmd.ExecuteNonQuery();
 }
 public void Delete(int ma) {
  using var cn=Db.CreateConnection();using var cmd=new SqlCommand("DELETE FROM dbo.NhaCungCap WHERE MaNCC=@Ma",cn);
  cmd.Parameters.Add("@Ma",SqlDbType.Int).Value=ma;cn.Open();cmd.ExecuteNonQuery();
 }
}
