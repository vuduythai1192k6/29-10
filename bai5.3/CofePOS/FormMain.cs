using System.Data;
using Microsoft.Data.SqlClient;

namespace CafePOS;

public sealed class FormMain : Form
{
    private readonly FlowLayoutPanel flpTables = new();
    private readonly ComboBox cboCategory = new();
    private readonly ComboBox cboProduct = new();
    private readonly NumericUpDown nudQuantity = new();
    private readonly DataGridView dgvOrder = new();
    private readonly Label lblSelectedTable = new();
    private readonly Label lblTotal = new();
    private readonly Button btnAdd = new();
    private readonly Button btnPay = new();
    private readonly Button btnChangeTable = new();
    private readonly Button btnMenu = new();
    private readonly Button btnRefresh = new();

    private DataTable? products;
    private DataTable? orderTable;
    private int? selectedTableId;
    private string? selectedTableName;
    private int? activeInvoiceId;

    public FormMain()
    {
        Text = "CafePOS - Hệ thống quản lý quán cafe";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 650);
        Size = new Size(1250, 780);
        Font = new Font("Segoe UI", 10);
        BuildUi();
        Shown += (_, _) => LoadAll();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(10) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        Controls.Add(root);

        var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
        var right = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
        root.Controls.Add(left, 0, 0);
        root.Controls.Add(right, 1, 0);

        var tableTitle = new Label { Text = "SƠ ĐỒ BÀN", Dock = DockStyle.Top, Height = 38, Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        flpTables.Dock = DockStyle.Fill;
        flpTables.AutoScroll = true;
        flpTables.Padding = new Padding(5);
        left.Controls.Add(flpTables);
        left.Controls.Add(tableTitle);

        var legend = new Label { Text = "  ● Trống       ● Có khách", Dock = DockStyle.Bottom, Height = 32, ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft };
        left.Controls.Add(legend);

        var header = new Label { Text = "BÁN HÀNG / GỌI MÓN", Dock = DockStyle.Top, Height = 38, Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        right.Controls.Add(header);

        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(0, 4, 0, 0) };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        right.Controls.Add(content);

        lblSelectedTable.Text = "Chưa chọn bàn";
        lblSelectedTable.Dock = DockStyle.Fill;
        lblSelectedTable.TextAlign = ContentAlignment.MiddleLeft;
        content.Controls.Add(lblSelectedTable, 0, 0);

        var orderControls = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Padding = new Padding(0, 5, 0, 5) };
        orderControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        orderControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37));
        orderControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        orderControls.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
        orderControls.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        orderControls.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        orderControls.Controls.Add(new Label { Text = "Danh mục", Dock = DockStyle.Fill }, 0, 0);
        orderControls.Controls.Add(new Label { Text = "Món ăn", Dock = DockStyle.Fill }, 1, 0);
        orderControls.Controls.Add(new Label { Text = "SL", Dock = DockStyle.Fill }, 2, 0);
        orderControls.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill }, 3, 0);
        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cboProduct.DropDownStyle = ComboBoxStyle.DropDownList;
        nudQuantity.Minimum = 1;
        nudQuantity.Maximum = 999;
        btnAdd.Text = "Thêm món";
        btnAdd.Dock = DockStyle.Fill;
        btnAdd.Click += (_, _) => AddProduct();
        orderControls.Controls.Add(cboCategory, 0, 1);
        orderControls.Controls.Add(cboProduct, 1, 1);
        orderControls.Controls.Add(nudQuantity, 2, 1);
        orderControls.Controls.Add(btnAdd, 3, 1);
        content.Controls.Add(orderControls, 0, 1);

        dgvOrder.Dock = DockStyle.Fill;
        dgvOrder.ReadOnly = true;
        dgvOrder.AllowUserToAddRows = false;
        dgvOrder.AllowUserToDeleteRows = false;
        dgvOrder.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvOrder.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvOrder.MultiSelect = false;
        content.Controls.Add(dgvOrder, 0, 2);

        lblTotal.Text = "Tổng tiền: 0 đ";
        lblTotal.Dock = DockStyle.Fill;
        lblTotal.TextAlign = ContentAlignment.MiddleRight;
        lblTotal.Font = new Font(Font, FontStyle.Bold);
        content.Controls.Add(lblTotal, 0, 3);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
        btnPay.Text = "Thanh toán";
        btnPay.Width = 125;
        btnPay.Click += (_, _) => PayInvoice();
        btnChangeTable.Text = "Đổi bàn";
        btnChangeTable.Width = 100;
        btnChangeTable.Click += (_, _) => ChangeTable();
        btnMenu.Text = "Quản lý thực đơn";
        btnMenu.Width = 165;
        btnMenu.Click += (_, _) => { using var f = new FormMenu(); f.ShowDialog(this); LoadProducts(); LoadTables(); };
        btnRefresh.Text = "Làm mới";
        btnRefresh.Width = 95;
        btnRefresh.Click += (_, _) => LoadAll();
        actions.Controls.AddRange(new Control[] { btnPay, btnChangeTable, btnMenu, btnRefresh });
        content.Controls.Add(actions, 0, 4);

        cboCategory.SelectedIndexChanged += (_, _) => FilterProducts();
    }

    private void LoadAll()
    {
        try
        {
            LoadCategories();
            LoadProducts();
            LoadTables();
            if (selectedTableId.HasValue) LoadOrder(selectedTableId.Value);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không tải được dữ liệu. Hãy kiểm tra kết nối và CSDL CafePOSDB.\n\n" + ex.Message,
                "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadCategories()
    {
        using var conn = Database.CreateConnection();
        using var adapter = new SqlDataAdapter("SELECT MaDanhMuc, TenDanhMuc FROM DanhMuc ORDER BY TenDanhMuc", conn);
        var dt = new DataTable();
        adapter.Fill(dt);
        cboCategory.DataSource = dt;
        cboCategory.DisplayMember = "TenDanhMuc";
        cboCategory.ValueMember = "MaDanhMuc";
    }

    private void LoadProducts()
    {
        using var conn = Database.CreateConnection();
        using var adapter = new SqlDataAdapter("SELECT MaMon, TenMon, MaDanhMuc, DonGia FROM MonAn ORDER BY TenMon", conn);
        products = new DataTable();
        adapter.Fill(products);
        FilterProducts();
    }

    private void FilterProducts()
    {
        if (products == null) return;
        var dt = products.Clone();
        if (cboCategory.SelectedValue != null && int.TryParse(cboCategory.SelectedValue.ToString(), out int categoryId))
        {
            foreach (DataRow row in products.Select($"MaDanhMuc = {categoryId}"))
                dt.ImportRow(row);
        }
        else
        {
            dt = products.Copy();
        }
        cboProduct.DataSource = dt;
        cboProduct.DisplayMember = "TenMon";
        cboProduct.ValueMember = "MaMon";
    }

    private void LoadTables()
    {
        using var conn = Database.CreateConnection();
        using var adapter = new SqlDataAdapter("SELECT MaBan, TenBan, TrangThai FROM BanAn ORDER BY MaBan", conn);
        var dt = new DataTable();
        adapter.Fill(dt);
        flpTables.Controls.Clear();

        foreach (DataRow row in dt.Rows)
        {
            int tableId = Convert.ToInt32(row["MaBan"]);
            string name = Convert.ToString(row["TenBan"]) ?? $"Bàn {tableId}";
            string status = Convert.ToString(row["TrangThai"]) ?? "Trống";
            var button = new Button
            {
                Text = name + "\n" + status,
                Width = 125,
                Height = 78,
                Margin = new Padding(7),
                BackColor = status == "Có khách" ? Color.IndianRed : Color.MediumSeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Tag = tableId
            };
            button.Click += (_, _) => SelectTable(tableId, name);
            flpTables.Controls.Add(button);
        }
    }

    private void SelectTable(int tableId, string tableName)
    {
        selectedTableId = tableId;
        selectedTableName = tableName;
        lblSelectedTable.Text = $"Đang chọn: {tableName}";
        LoadOrder(tableId);
    }

    private void LoadOrder(int tableId)
    {
        using var conn = Database.CreateConnection();
        conn.Open();

        using (var cmd = new SqlCommand("SELECT TOP 1 MaHoaDon FROM HoaDon WHERE MaBan=@MaBan AND TrangThai=0 ORDER BY MaHoaDon DESC", conn))
        {
            cmd.Parameters.AddWithValue("@MaBan", tableId);
            var result = cmd.ExecuteScalar();
            activeInvoiceId = result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
        }

        orderTable = new DataTable();
        if (activeInvoiceId.HasValue)
        {
            using var adapter = new SqlDataAdapter(
                @"SELECT ct.MaMon, m.TenMon, ct.SoLuong, ct.DonGia,
                         CAST(ct.SoLuong * ct.DonGia AS DECIMAL(18,2)) AS ThanhTien
                  FROM ChiTietHoaDon ct
                  INNER JOIN MonAn m ON m.MaMon=ct.MaMon
                  WHERE ct.MaHoaDon=@MaHoaDon ORDER BY m.TenMon", conn);
            adapter.SelectCommand!.Parameters.AddWithValue("@MaHoaDon", activeInvoiceId.Value);
            adapter.Fill(orderTable);
        }
        else
        {
            orderTable.Columns.Add("MaMon", typeof(int));
            orderTable.Columns.Add("TenMon", typeof(string));
            orderTable.Columns.Add("SoLuong", typeof(int));
            orderTable.Columns.Add("DonGia", typeof(decimal));
            orderTable.Columns.Add("ThanhTien", typeof(decimal));
        }

        dgvOrder.DataSource = orderTable;
        if (dgvOrder.Columns["MaMon"] != null) dgvOrder.Columns["MaMon"]!.Visible = false;
        FormatCurrencyColumns();
        UpdateTotal();
    }

    private void FormatCurrencyColumns()
    {
        foreach (string col in new[] { "DonGia", "ThanhTien" })
            if (dgvOrder.Columns[col] != null)
                dgvOrder.Columns[col]!.DefaultCellStyle.Format = "N0";
    }

    private void UpdateTotal()
    {
        decimal total = 0;
        if (orderTable != null && orderTable.Columns.Contains("ThanhTien"))
            foreach (DataRow row in orderTable.Rows)
                if (row.RowState != DataRowState.Deleted && row["ThanhTien"] != DBNull.Value)
                    total += Convert.ToDecimal(row["ThanhTien"]);
        lblTotal.Text = $"Tổng tiền: {total:N0} đ";
    }

    private void AddProduct()
    {
        if (!selectedTableId.HasValue)
        {
            MessageBox.Show("Anh hãy chọn một bàn trước.", "Chưa chọn bàn");
            return;
        }
        if (cboProduct.SelectedValue == null || !int.TryParse(cboProduct.SelectedValue.ToString(), out int productId))
        {
            MessageBox.Show("Vui lòng chọn món ăn.");
            return;
        }

        int quantity = (int)nudQuantity.Value;
        decimal price = Convert.ToDecimal(((DataRowView)cboProduct.SelectedItem!)["DonGia"]);

        using var conn = Database.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            int invoiceId;
            using (var find = new SqlCommand("SELECT TOP 1 MaHoaDon FROM HoaDon WHERE MaBan=@MaBan AND TrangThai=0 ORDER BY MaHoaDon DESC", conn, tx))
            {
                find.Parameters.AddWithValue("@MaBan", selectedTableId.Value);
                object? value = find.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                {
                    using var create = new SqlCommand(
                        "INSERT INTO HoaDon(MaBan, GioVao, TongTien, TrangThai) VALUES(@MaBan, GETDATE(), 0, 0); SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx);
                    create.Parameters.AddWithValue("@MaBan", selectedTableId.Value);
                    invoiceId = Convert.ToInt32(create.ExecuteScalar());
                }
                else invoiceId = Convert.ToInt32(value);
            }

            using (var add = new SqlCommand(@"
IF EXISTS (SELECT 1 FROM ChiTietHoaDon WHERE MaHoaDon=@MaHoaDon AND MaMon=@MaMon)
    UPDATE ChiTietHoaDon SET SoLuong=SoLuong+@SoLuong WHERE MaHoaDon=@MaHoaDon AND MaMon=@MaMon;
ELSE
    INSERT INTO ChiTietHoaDon(MaHoaDon, MaMon, SoLuong, DonGia) VALUES(@MaHoaDon, @MaMon, @SoLuong, @DonGia);", conn, tx))
            {
                add.Parameters.AddWithValue("@MaHoaDon", invoiceId);
                add.Parameters.AddWithValue("@MaMon", productId);
                add.Parameters.AddWithValue("@SoLuong", quantity);
                add.Parameters.AddWithValue("@DonGia", price);
                add.ExecuteNonQuery();
            }

            using (var status = new SqlCommand("UPDATE BanAn SET TrangThai=N'Có khách' WHERE MaBan=@MaBan", conn, tx))
            {
                status.Parameters.AddWithValue("@MaBan", selectedTableId.Value);
                status.ExecuteNonQuery();
            }

            tx.Commit();
            LoadTables();
            LoadOrder(selectedTableId.Value);
        }
        catch (Exception ex)
        {
            try { tx.Rollback(); } catch { }
            MessageBox.Show("Không thể thêm món: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PayInvoice()
    {
        if (!selectedTableId.HasValue || !activeInvoiceId.HasValue)
        {
            MessageBox.Show("Bàn này chưa có hóa đơn cần thanh toán.");
            return;
        }

        using var conn = Database.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            decimal total;
            using (var sum = new SqlCommand(
                "SELECT COALESCE(SUM(CAST(SoLuong AS DECIMAL(18,2))*DonGia),0) FROM ChiTietHoaDon WHERE MaHoaDon=@MaHoaDon", conn, tx))
            {
                sum.Parameters.AddWithValue("@MaHoaDon", activeInvoiceId.Value);
                total = Convert.ToDecimal(sum.ExecuteScalar());
            }

            using (var invoice = new SqlCommand(
                "UPDATE HoaDon SET GioRa=GETDATE(), TongTien=@TongTien, TrangThai=1 WHERE MaHoaDon=@MaHoaDon AND TrangThai=0", conn, tx))
            {
                invoice.Parameters.AddWithValue("@TongTien", total);
                invoice.Parameters.AddWithValue("@MaHoaDon", activeInvoiceId.Value);
                if (invoice.ExecuteNonQuery() != 1) throw new InvalidOperationException("Hóa đơn đã được thanh toán hoặc không còn tồn tại.");
            }

            using (var table = new SqlCommand("UPDATE BanAn SET TrangThai=N'Trống' WHERE MaBan=@MaBan", conn, tx))
            {
                table.Parameters.AddWithValue("@MaBan", selectedTableId.Value);
                table.ExecuteNonQuery();
            }

            tx.Commit();
            MessageBox.Show($"Thanh toán thành công!\nTổng tiền: {total:N0} đ", "Thanh toán");
            LoadTables();
            LoadOrder(selectedTableId.Value);
        }
        catch (Exception ex)
        {
            try { tx.Rollback(); } catch { }
            MessageBox.Show("Thanh toán thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ChangeTable()
    {
        if (!selectedTableId.HasValue || !activeInvoiceId.HasValue)
        {
            MessageBox.Show("Bàn đang chọn chưa có hóa đơn để đổi.");
            return;
        }

        var freeTables = new DataTable();
        using (var conn = Database.CreateConnection())
        using (var adapter = new SqlDataAdapter(
            "SELECT MaBan, TenBan FROM BanAn WHERE TrangThai=N'Trống' AND MaBan<>@MaBan ORDER BY MaBan", conn))
        {
            adapter.SelectCommand!.Parameters.AddWithValue("@MaBan", selectedTableId.Value);
            adapter.Fill(freeTables);
        }
        if (freeTables.Rows.Count == 0)
        {
            MessageBox.Show("Không có bàn trống để chuyển đến.");
            return;
        }

        using var dialog = new Form { Text = "Đổi bàn", StartPosition = FormStartPosition.CenterParent, Size = new Size(330, 140), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
        var combo = new ComboBox { Left = 15, Top = 15, Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, DataSource = freeTables, DisplayMember = "TenBan", ValueMember = "MaBan" };
        var ok = new Button { Text = "Chuyển bàn", Left = 175, Top = 55, Width = 120, DialogResult = DialogResult.OK };
        dialog.Controls.Add(combo);
        dialog.Controls.Add(ok);
        dialog.AcceptButton = ok;
        if (dialog.ShowDialog(this) != DialogResult.OK || combo.SelectedValue == null) return;

        int destinationId = Convert.ToInt32(combo.SelectedValue);
        using var connection = Database.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            using (var updateInvoice = new SqlCommand("UPDATE HoaDon SET MaBan=@NewBan WHERE MaHoaDon=@MaHoaDon AND TrangThai=0", connection, transaction))
            {
                updateInvoice.Parameters.AddWithValue("@NewBan", destinationId);
                updateInvoice.Parameters.AddWithValue("@MaHoaDon", activeInvoiceId.Value);
                updateInvoice.ExecuteNonQuery();
            }
            using (var updateOld = new SqlCommand("UPDATE BanAn SET TrangThai=N'Trống' WHERE MaBan=@MaBan", connection, transaction))
            {
                updateOld.Parameters.AddWithValue("@MaBan", selectedTableId.Value);
                updateOld.ExecuteNonQuery();
            }
            using (var updateNew = new SqlCommand("UPDATE BanAn SET TrangThai=N'Có khách' WHERE MaBan=@MaBan", connection, transaction))
            {
                updateNew.Parameters.AddWithValue("@MaBan", destinationId);
                updateNew.ExecuteNonQuery();
            }
            transaction.Commit();

            var selected = freeTables.Select($"MaBan = {destinationId}")[0];
            selectedTableId = destinationId;
            selectedTableName = Convert.ToString(selected["TenBan"]);
            lblSelectedTable.Text = $"Đang chọn: {selectedTableName}";
            LoadTables();
            LoadOrder(destinationId);
        }
        catch (Exception ex)
        {
            try { transaction.Rollback(); } catch { }
            MessageBox.Show("Đổi bàn thất bại: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
