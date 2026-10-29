using System.Data;
using HotelManagement.BUS;
using HotelManagement.DTO;

namespace HotelManagement.GUI;

public class frmMain : Form
{
    private readonly HotelBUS bus = new();
    private readonly TabControl tabs = new() { Dock = DockStyle.Fill };
    private readonly DataGridView dgvRooms = Grid();
    private readonly DataGridView dgvBookings = Grid();
    private readonly DataGridView dgvServices = Grid();
    private readonly ComboBox cboRoom = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
    private readonly ComboBox cboService = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox cboActiveBooking = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly ComboBox cboTypeFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
    private readonly ComboBox cboStatusFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly TextBox txtName = new() { Width = 180 };
    private readonly TextBox txtCCCD = new() { Width = 150 };
    private readonly TextBox txtPhone = new() { Width = 140 };
    private readonly NumericUpDown numQuantity = new() { Minimum = 1, Maximum = 100, Value = 1, Width = 80 };
    private DataTable? roomTable;
    private DataView? roomView;
    private List<BookingDTO> activeBookings = new();
    private List<ServiceDTO> services = new();

    public frmMain()
    {
        Text = "Hotel Management System - 3 Tier";
        Width = 1150; Height = 760; MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BuildUI();
        Load += (_, _) => ReloadAll();
    }

    private static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, BackgroundColor = SystemColors.Window
    };

    private static Control Field(string label, Control input)
    {
        var p = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(8) };
        p.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 0, 0, 4) });
        p.Controls.Add(input); return p;
    }

    private void BuildUI()
    {
        var tabCheckin = new TabPage("Check-in");
        var form = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12), WrapContents = true };
        form.Controls.Add(Field("Họ tên khách", txtName));
        form.Controls.Add(Field("CCCD", txtCCCD));
        form.Controls.Add(Field("Số điện thoại", txtPhone));
        form.Controls.Add(Field("Phòng trống", cboRoom));
        var btnCheckIn = new Button { Text = "Nhận phòng", AutoSize = true, Height = 35, Margin = new Padding(10, 28, 10, 10) };
        btnCheckIn.Click += (_, _) => DoCheckIn();
        form.Controls.Add(btnCheckIn);
        tabCheckin.Controls.Add(dgvRooms);
        tabCheckin.Controls.Add(form);

        var tabService = new TabPage("Dịch vụ");
        var serviceTop = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12), WrapContents = true };
        serviceTop.Controls.Add(Field("Phiếu đang ở", cboActiveBooking));
        serviceTop.Controls.Add(Field("Dịch vụ", cboService));
        serviceTop.Controls.Add(Field("Số lượng", numQuantity));
        var btnAddService = new Button { Text = "Gọi dịch vụ", AutoSize = true, Height = 35, Margin = new Padding(10, 28, 10, 10) };
        btnAddService.Click += (_, _) => DoAddService();
        serviceTop.Controls.Add(btnAddService);
        var serviceLayout = new Panel { Dock = DockStyle.Fill };
        serviceLayout.Controls.Add(dgvServices); serviceLayout.Controls.Add(serviceTop);
        tabService.Controls.Add(serviceLayout);

        var tabCheckout = new TabPage("Check-out");
        var checkoutTop = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12) };
        var btnRefreshBookings = new Button { Text = "Tải phiếu đang ở", AutoSize = true };
        btnRefreshBookings.Click += (_, _) => LoadBookings();
        var btnCheckout = new Button { Text = "Check-out & xuất hóa đơn", AutoSize = true };
        btnCheckout.Click += (_, _) => DoCheckOut();
        checkoutTop.Controls.Add(Field("Phiếu đang lưu trú", cboActiveBooking));
        checkoutTop.Controls.Add(btnRefreshBookings); checkoutTop.Controls.Add(btnCheckout);
        tabCheckout.Controls.Add(dgvBookings); tabCheckout.Controls.Add(checkoutTop);

        var tabRooms = new TabPage("Lọc phòng");

        var filterTop = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(12) };
        cboTypeFilter.Items.AddRange(new object[] { "Tất cả loại", "Đơn", "Đôi", "VIP" }); cboTypeFilter.SelectedIndex = 0;
        cboStatusFilter.Items.AddRange(new object[] { "Tất cả trạng thái", "Trống", "Đang ở", "Đang dọn dẹp" }); cboStatusFilter.SelectedIndex = 0;
        cboTypeFilter.SelectedIndexChanged += (_, _) => ApplyRoomFilter();
        cboStatusFilter.SelectedIndexChanged += (_, _) => ApplyRoomFilter();

        filterTop.Controls.Add(Field("Loại phòng", cboTypeFilter)); 

        filterTop.Controls.Add(Field("Trạng thái", cboStatusFilter));
        var btnRefreshRooms = new Button { Text = "Làm mới", AutoSize = true, Margin = new Padding(8, 25, 8, 8) };
        btnRefreshRooms.Click += (_, _) => LoadRooms();
        filterTop.Controls.Add(btnRefreshRooms);
        var filterPanel = new Panel { Dock = DockStyle.Fill };
        filterPanel.Controls.Add(dgvRooms); filterPanel.Controls.Add(filterTop);
        tabRooms.Controls.Add(filterPanel);

        tabs.TabPages.AddRange(new[] { tabCheckin, tabService, tabCheckout, tabRooms });
        Controls.Add(tabs);
    }

    private void ReloadAll()
    {
        try
        {
            LoadRooms();
            LoadServices();
            LoadBookings();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void LoadRooms()
    {
        roomTable = bus.GetRooms();
        roomView = new DataView(roomTable);
        dgvRooms.DataSource = roomView;
        cboRoom.DataSource = bus.GetAvailableRooms();
        cboRoom.DisplayMember = "SoPhong";
        cboRoom.ValueMember = "SoPhong";
        ApplyRoomFilter();
    }

    private void ApplyRoomFilter()
    {
        if (roomView == null) return;
        var filters = new List<string>();
        if (cboTypeFilter.SelectedIndex > 0)
            filters.Add($"LoaiPhong = '{cboTypeFilter.Text.Replace("'", "''")}'");
        if (cboStatusFilter.SelectedIndex > 0)
            filters.Add($"TrangThai = '{cboStatusFilter.Text.Replace("'", "''")}'");
        roomView.RowFilter = string.Join(" AND ", filters);
    }

    private void LoadServices()
    {
        services = bus.GetServices();
        cboService.DataSource = services;
        cboService.DisplayMember = "TenDV";
        cboService.ValueMember = "MaDV";
        dgvServices.DataSource = bus.GetServiceTable();
    }

    private void LoadBookings()
    {
        activeBookings = bus.GetActiveBookings();
        cboActiveBooking.DataSource = null;
        cboActiveBooking.DataSource = activeBookings;
        cboActiveBooking.DisplayMember = "DisplayText";
        cboActiveBooking.ValueMember = "MaPhieu";
        
        dgvBookings.DataSource = activeBookings.Select(b => new {
            b.MaPhieu, b.SoPhong, b.TenKhach, b.NgayNhanPhong
        }).ToList();
    }

    private void DoCheckIn()
    {
        try
        {
            if (cboRoom.SelectedValue == null) throw new InvalidOperationException("Không có phòng trống.");
            var customer = bus.GetOrCreateCustomer(txtName.Text, txtCCCD.Text, txtPhone.Text);
            bus.CheckIn(customer.MaKH, cboRoom.SelectedValue.ToString()!);
            MessageBox.Show($"Nhận phòng thành công. Mã khách: {customer.MaKH}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtName.Clear(); txtCCCD.Clear(); txtPhone.Clear();
            ReloadAll();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void DoAddService()
    {
        try
        {
            if (cboActiveBooking.SelectedItem is not BookingDTO booking) throw new InvalidOperationException("Chọn phiếu đang lưu trú.");
            if (cboService.SelectedItem is not ServiceDTO service) throw new InvalidOperationException("Chọn dịch vụ.");
            bus.AddService(booking.MaPhieu, service.MaDV, (int)numQuantity.Value);
            MessageBox.Show("Đã ghi nhận dịch vụ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void DoCheckOut()
    {
        try
        {
            if (cboActiveBooking.SelectedItem is not BookingDTO booking) throw new InvalidOperationException("Chọn phiếu cần check-out.");
            var confirm = MessageBox.Show($"Xác nhận check-out phòng {booking.SoPhong}?", "Xác nhận thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            var total = bus.CheckOut(booking.MaPhieu);
            MessageBox.Show($"CHECK-OUT THÀNH CÔNG\nMã phiếu: {booking.MaPhieu}\nPhòng: {booking.SoPhong}\nKhách: {booking.TenKhach}\nTổng thanh toán: {total:N0} VNĐ", "Hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ReloadAll();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private static void ShowError(Exception ex) =>
        MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
