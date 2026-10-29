using System.Data;
using InventoryManagementWinForms.BUS;
using InventoryManagementWinForms.DTO;
namespace InventoryManagementWinForms;
public sealed class MainForm : Form {
 readonly VatTuBUS vb=new(); readonly NhaCungCapBUS nb=new(); readonly PhieuKhoBUS pb=new();
 readonly TabControl tabs=new(){Dock=DockStyle.Fill};
 readonly DataGridView gv=Grid(),gn=Grid(),gd=Grid(),gl=Grid();
 readonly TextBox ma=new(),ten=new(),dvt=new(),search=new(),nTen=new(),nSdt=new(),nDc=new(),ctMa=new();
 readonly NumericUpDown ton=Num(),sl=Num(1),gia=Num(),nguong=Num(10);
 readonly ComboBox loai=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=100}, ncc=new(){DropDownStyle=ComboBoxStyle.DropDownList,Width=220};
 readonly Label total=new(){AutoSize=true};
 bool editVatTu=false,editNcc=false; int nccId;
 static NumericUpDown Num(decimal value=0)=>new(){DecimalPlaces=2,Minimum=0,Maximum=100000000,Value=value,Width=100};
 static DataGridView Grid()=>new(){Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,SelectionMode=DataGridViewSelectionMode.FullRowSelect,MultiSelect=false};
 static Button B(string t,Action a){var b=new Button{Text=t,AutoSize=true,Margin=new Padding(4)};b.Click+=(_,_)=>a();return b;}
 static void Field(FlowLayoutPanel p,string l,Control c){p.Controls.Add(new Label{Text=l,AutoSize=true,Padding=new Padding(3,8,3,0)});p.Controls.Add(c);}
 public MainForm(){
  Text="Quản lý quy trình xuất - nhập kho";Width=1150;Height=720;StartPosition=FormStartPosition.CenterScreen;
  BuildVatTu();BuildNcc();BuildPhieu();BuildReport();Controls.Add(tabs);
  try{RefreshAll();}catch(Exception e){MessageBox.Show("Kiểm tra SQL Server và chạy Database/InventoryDB.sql trước.\n"+e.Message);}
 }
 void BuildVatTu(){
  var p=new TabPage("Vật tư");var l=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=3,ColumnCount=1};l.RowStyles.Add(new RowStyle(SizeType.AutoSize));l.RowStyles.Add(new RowStyle(SizeType.AutoSize));l.RowStyles.Add(new RowStyle(SizeType.Percent,100));
  var s=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,Padding=new Padding(6)};Field(s,"Tìm mã/tên",search);s.Controls.Add(B("Tìm",LoadVatTu));s.Controls.Add(B("Tất cả",()=>{search.Clear();LoadVatTu();}));
  var f=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,Padding=new Padding(6)};Field(f,"Mã",ma);Field(f,"Tên",ten);Field(f,"ĐVT",dvt);Field(f,"Tồn",ton);f.Controls.Add(B("Thêm mới",ClearVatTu));f.Controls.Add(B("Lưu",SaveVatTu));f.Controls.Add(B("Xóa",DeleteVatTu));gv.SelectionChanged+=(_,_)=>SelectVatTu();
  l.Controls.Add(s,0,0);l.Controls.Add(f,0,1);l.Controls.Add(gv,0,2);p.Controls.Add(l);tabs.TabPages.Add(p);
 }
 void BuildNcc(){
  var p=new TabPage("Nhà cung cấp");var l=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=2,ColumnCount=1};l.RowStyles.Add(new RowStyle(SizeType.AutoSize));l.RowStyles.Add(new RowStyle(SizeType.Percent,100));
  var f=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,Padding=new Padding(6)};Field(f,"Mã (chọn từ bảng)",new Label{Text="Mã tự động",AutoSize=true});Field(f,"Tên",nTen);Field(f,"Điện thoại",nSdt);Field(f,"Địa chỉ",nDc);f.Controls.Add(B("Thêm mới",ClearNcc));f.Controls.Add(B("Lưu",SaveNcc));f.Controls.Add(B("Xóa",DeleteNcc));gn.SelectionChanged+=(_,_)=>SelectNcc();
  l.Controls.Add(f,0,0);l.Controls.Add(gn,0,1);p.Controls.Add(l);tabs.TabPages.Add(p);
 }
 void BuildPhieu(){
  var p=new TabPage("Phiếu nhập / xuất");var l=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=3,ColumnCount=1};l.RowStyles.Add(new RowStyle(SizeType.AutoSize));l.RowStyles.Add(new RowStyle(SizeType.AutoSize));l.RowStyles.Add(new RowStyle(SizeType.Percent,100));
  var h=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,Padding=new Padding(6)};Field(h,"Loại phiếu",loai);loai.Items.AddRange(new object[]{"NHAP","XUAT"});loai.SelectedIndex=0;Field(h,"Nhà cung cấp (phiếu nhập)",ncc);
  var f=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,Padding=new Padding(6)};Field(f,"Mã vật tư",ctMa);Field(f,"Số lượng",sl);Field(f,"Đơn giá",gia);f.Controls.Add(B("Thêm dòng",AddDetail));f.Controls.Add(B("Xóa dòng",()=>{if(gd.CurrentRow!=null&&!gd.CurrentRow.IsNewRow)gd.Rows.Remove(gd.CurrentRow);}));f.Controls.Add(B("Lưu phiếu",SavePhieu));f.Controls.Add(B("Xóa tất cả",()=>gd.Rows.Clear()));
  gd.Columns.Add("Ma","Mã vật tư");gd.Columns.Add("SL","Số lượng");gd.Columns.Add("Gia","Đơn giá");gd.Columns.Add("TT","Thành tiền");
  l.Controls.Add(h,0,0);l.Controls.Add(f,0,1);l.Controls.Add(gd,0,2);p.Controls.Add(l);tabs.TabPages.Add(p);
 }
 void BuildReport(){
  var p=new TabPage("Cảnh báo & báo cáo");var l=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=3,ColumnCount=1};l.RowStyles.Add(new RowStyle(SizeType.AutoSize));l.RowStyles.Add(new RowStyle(SizeType.AutoSize));l.RowStyles.Add(new RowStyle(SizeType.Percent,100));
  var f=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,Padding=new Padding(6)};Field(f,"Ngưỡng tồn thấp",nguong);f.Controls.Add(B("Lọc tồn thấp",LoadLow));f.Controls.Add(B("Tổng giá trị tồn",LoadTotal));
  var t=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoSize=true,Padding=new Padding(6)};t.Controls.Add(total);l.Controls.Add(f,0,0);l.Controls.Add(t,0,1);l.Controls.Add(gl,0,2);p.Controls.Add(l);tabs.TabPages.Add(p);
 }
 void RefreshAll(){LoadVatTu();LoadNcc();LoadLow();LoadTotal();}
 void LoadVatTu()=>gv.DataSource=vb.Search(search.Text);
 void ClearVatTu(){editVatTu=false;ma.ReadOnly=false;ma.Clear();ten.Clear();dvt.Clear();ton.Value=0;}
 void SelectVatTu(){if(gv.CurrentRow?.DataBoundItem is not DataRowView r)return;editVatTu=true;ma.Text=Convert.ToString(r["MaVatTu"]);ma.ReadOnly=true;ten.Text=Convert.ToString(r["TenVatTu"]);dvt.Text=Convert.ToString(r["DVT"]);ton.Value=Convert.ToDecimal(r["SoLuongTon"]);}
 void SaveVatTu(){try{vb.Save(new VatTuDTO{MaVatTu=ma.Text.Trim(),TenVatTu=ten.Text.Trim(),DVT=dvt.Text.Trim(),SoLuongTon=ton.Value},!editVatTu);LoadVatTu();LoadLow();LoadTotal();MessageBox.Show("Đã lưu vật tư.");}catch(Exception e){MessageBox.Show(e.Message);}}
 void DeleteVatTu(){if(ma.Text.Length==0)return;try{vb.Delete(ma.Text.Trim());ClearVatTu();LoadVatTu();}catch(Exception e){MessageBox.Show("Không thể xóa vật tư đang được dùng trong phiếu. "+e.Message);}}
 void LoadNcc(){gn.DataSource=nb.GetAll();var dt=nb.GetAll();ncc.DataSource=dt;ncc.DisplayMember="TenNCC";ncc.ValueMember="MaNCC";ncc.SelectedIndex=dt.Rows.Count>0?0:-1;}
 void ClearNcc(){editNcc=false;nTen.Clear();nSdt.Clear();nDc.Clear();}
 void SelectNcc(){if(gn.CurrentRow?.DataBoundItem is not DataRowView r)return;editNcc=true;nccId=Convert.ToInt32(r["MaNCC"]);nTen.Text=Convert.ToString(r["TenNCC"]);nSdt.Text=Convert.ToString(r["DienThoai"]);nDc.Text=Convert.ToString(r["DiaChi"]);}
 void SaveNcc(){try{nb.Save(new NhaCungCapDTO{MaNCC=nccId,TenNCC=nTen.Text.Trim(),DienThoai=nSdt.Text.Trim(),DiaChi=nDc.Text.Trim()},!editNcc);LoadNcc();MessageBox.Show("Đã lưu nhà cung cấp.");}catch(Exception e){MessageBox.Show(e.Message);}}
 void DeleteNcc(){if(!editNcc)return;try{nb.Delete(nccId);ClearNcc();LoadNcc();}catch(Exception e){MessageBox.Show("Không thể xóa nhà cung cấp đang được phiếu nhập tham chiếu. "+e.Message);}}
 void AddDetail(){string m=ctMa.Text.Trim();if(m==""){MessageBox.Show("Nhập mã vật tư.");return;}if(gd.Rows.Cast<DataGridViewRow>().Any(r=>!r.IsNewRow&&Convert.ToString(r.Cells[0].Value)==m)){MessageBox.Show("Mã vật tư đã có trong phiếu.");return;}gd.Rows.Add(m,sl.Value,gia.Value,sl.Value*gia.Value);ctMa.Clear();}
 void SavePhieu(){try{var items=new List<ChiTietPhieuDTO>();foreach(DataGridViewRow r in gd.Rows)if(!r.IsNewRow)items.Add(new ChiTietPhieuDTO{MaVatTu=Convert.ToString(r.Cells[0].Value)??"",SoLuong=Convert.ToDecimal(r.Cells[1].Value),DonGia=Convert.ToDecimal(r.Cells[2].Value)});int? supplier=null;if(loai.Text=="NHAP"){if(ncc.SelectedValue==null)throw new Exception("Hãy thêm nhà cung cấp trước.");supplier=Convert.ToInt32(ncc.SelectedValue);}int id=pb.Save(new PhieuKhoDTO{LoaiPhieu=loai.Text,MaNCC=supplier,ChiTiet=items});MessageBox.Show("Lưu phiếu thành công. Mã phiếu: "+id);gd.Rows.Clear();LoadVatTu();LoadLow();LoadTotal();}catch(Exception e){MessageBox.Show(e.Message,"Không thể lưu phiếu",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
 void LoadLow()=>gl.DataSource=pb.LowStock(nguong.Value);
 void LoadTotal()=>total.Text="Tổng giá trị tồn kho (ước tính theo giá nhập gần nhất): "+pb.InventoryValue().ToString("N2");
}
