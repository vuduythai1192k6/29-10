using System.Data;
using Microsoft.Data.SqlClient;

namespace CafePOS;

public sealed class FormMenu : Form
{
    private readonly DataGridView dgv = new();
    private readonly TextBox txtName = new();
    private readonly ComboBox cboCategory = new();
    private readonly NumericUpDown nudPrice = new();
    private readonly Button btnAdd = new();
    private readonly Button btnUpdate = new();
    private readonly Button btnDelete = new();
    private readonly Button btnClear = new();
    private DataTable? menuTable;
    private int? selectedProductId;

    public FormMenu()
    {
        Text = "Quản lý thực đơn";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(850, 560);
        MinimumSize = new Size(750, 500);
        Font = new Font("Segoe UI", 10);
        BuildUi();
        Shown += (_, _) => LoadData();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10) };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
        Controls.Add(root);

        dgv.Dock = DockStyle.Fill;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.SelectionChanged += (_, _) => SelectCurrentRow();
        root.Controls.Add(dgv, 0, 0);

        var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(0, 10, 0, 0) };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        editor.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        editor.Controls.Add(new Label { Text = "Tên món", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
        txtName.Dock = DockStyle.Fill;
        editor.Controls.Add(txtName, 1, 0);
        editor.Controls.Add(new Label { Text = "Danh mục", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 2, 0);
        cboCategory.Dock = DockStyle.Fill;
        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        editor.Controls.Add(cboCategory, 3, 0);
        editor.Controls.Add(new Label { Text = "Đơn giá (đ)", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
        nudPrice.Dock = DockStyle.Fill;
        nudPrice.Minimum = 0;
        nudPrice.Maximum = 1000000000;
        nudPrice.DecimalPlaces = 2;
        nudPrice.ThousandsSeparator = true;
        editor.Controls.Add(nudPrice, 1, 1);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        btnAdd.Text = "Thêm";
        btnUpdate.Text = "Sửa";
        btnDelete.Text = "Xóa";
        btnClear.Text = "Làm mới";
        foreach (var button in new[] { btnAdd, btnUpdate, btnDelete, btnClear })
        {
            button.Width = 105;
            button.Height = 32;
            buttons.Controls.Add(button);
        }
        btnAdd.Click += (_, _) => SaveProduct(false);
        btnUpdate.Click += (_, _) => SaveProduct(true);
        btnDelete.Click += (_, _) => DeleteProduct();
        btnClear.Click += (_, _) => ClearEditor();
        editor.Controls.Add(buttons, 0, 2);
        editor.SetColumnSpan(buttons, 4);
        root.Controls.Add(editor, 0, 1);
    }

    private void LoadData()
    {
        try
        {
            using (var conn = Database.CreateConnection())
            using (var adapter = new SqlDataAdapter("SELECT MaDanhMuc, TenDanhMuc FROM DanhMuc ORDER BY TenDanhMuc", conn))
            {
                var categories = new DataTable();
                adapter.Fill(categories);
                cboCategory.DataSource = categories;
                cboCategory.DisplayMember = "TenDanhMuc";
                cboCategory.ValueMember = "MaDanhMuc";
            }

            using (var conn = Database.CreateConnection())
            using (var adapter = new SqlDataAdapter(
                @"SELECT m.MaMon, m.TenMon, m.MaDanhMuc, d.TenDanhMuc, m.DonGia
                  FROM MonAn m INNER JOIN DanhMuc d ON d.MaDanhMuc=m.MaDanhMuc
                  ORDER BY m.MaMon", conn))
            {
                menuTable = new DataTable();
                adapter.Fill(menuTable);
                dgv.DataSource = menuTable;
            }
            if (dgv.Columns["MaMon"] != null) dgv.Columns["MaMon"]!.HeaderText = "Mã món";
            if (dgv.Columns["TenMon"] != null) dgv.Columns["TenMon"]!.HeaderText = "Tên món";
            if (dgv.Columns["MaDanhMuc"] != null) dgv.Columns["MaDanhMuc"]!.Visible = false;
            if (dgv.Columns["TenDanhMuc"] != null) dgv.Columns["TenDanhMuc"]!.HeaderText = "Danh mục";
            if (dgv.Columns["DonGia"] != null)
            {
                dgv.Columns["DonGia"]!.HeaderText = "Đơn giá";
                dgv.Columns["DonGia"]!.DefaultCellStyle.Format = "N0";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không tải được thực đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SelectCurrentRow()
    {
        if (dgv.CurrentRow == null || dgv.CurrentRow.DataBoundItem is not DataRowView view) return;
        selectedProductId = Convert.ToInt32(view["MaMon"]);
        txtName.Text = Convert.ToString(view["TenMon"]) ?? "";
        nudPrice.Value = Math.Min(nudPrice.Maximum, Math.Max(nudPrice.Minimum, Convert.ToDecimal(view["DonGia"])));
        if (cboCategory.Items.Count > 0)
            cboCategory.SelectedValue = Convert.ToInt32(view["MaDanhMuc"]);
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Tên món không được để trống.");
            txtName.Focus();
            return false;
        }
        if (cboCategory.SelectedValue == null || !int.TryParse(cboCategory.SelectedValue.ToString(), out _))
        {
            MessageBox.Show("Vui lòng chọn danh mục.");
            return false;
        }
        if (nudPrice.Value < 0)
        {
            MessageBox.Show("Đơn giá không được âm.");
            return false;
        }
        return true;
    }

    private void SaveProduct(bool update)
    {
        if (!ValidateInput()) return;
        if (update && !selectedProductId.HasValue)
        {
            MessageBox.Show("Hãy chọn món cần sửa trong danh sách.");
            return;
        }

        try
        {
            using var conn = Database.CreateConnection();
            conn.Open();
            string sql = update
                ? "UPDATE MonAn SET TenMon=@TenMon, MaDanhMuc=@MaDanhMuc, DonGia=@DonGia WHERE MaMon=@MaMon"
                : "INSERT INTO MonAn(TenMon, MaDanhMuc, DonGia) VALUES(@TenMon, @MaDanhMuc, @DonGia)";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@TenMon", SqlDbType.NVarChar, 100).Value = txtName.Text.Trim();
            cmd.Parameters.Add("@MaDanhMuc", SqlDbType.Int).Value = Convert.ToInt32(cboCategory.SelectedValue);
            cmd.Parameters.Add("@DonGia", SqlDbType.Decimal).Value = nudPrice.Value;
            cmd.Parameters["@DonGia"].Precision = 18;
            cmd.Parameters["@DonGia"].Scale = 2;
            if (update) cmd.Parameters.Add("@MaMon", SqlDbType.Int).Value = selectedProductId!.Value;
            cmd.ExecuteNonQuery();
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không lưu được món: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteProduct()
    {
        if (!selectedProductId.HasValue)
        {
            MessageBox.Show("Hãy chọn món cần xóa.");
            return;
        }
        if (MessageBox.Show("Xóa món đã chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        try
        {
            using var conn = Database.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM MonAn WHERE MaMon=@MaMon", conn);
            cmd.Parameters.AddWithValue("@MaMon", selectedProductId.Value);
            cmd.ExecuteNonQuery();
            LoadData();
            ClearEditor();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không xóa được món. Món có thể đã xuất hiện trong hóa đơn.\n" + ex.Message,
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearEditor()
    {
        selectedProductId = null;
        txtName.Clear();
        nudPrice.Value = 0;
        if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
        dgv.ClearSelection();
    }
}
