using System.Windows;
using System.Windows.Controls;
using System.Linq;
using MES.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MES.Models;
using MES.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MES.Views
{
    /// <summary>
    /// Interaction logic for Orders.xaml
    /// </summary>
    public partial class Orders : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private int _totalOrders;
        private int _pendingOrders;
        private int _processingOrders;
        private int _completedOrders;

        public int TotalOrders
        {
            get => _totalOrders;
            set { _totalOrders = value; OnPropertyChanged(); }
        }

        public int PendingOrders
        {
            get => _pendingOrders;
            set { _pendingOrders = value; OnPropertyChanged(); }
        }

        public int ProcessingOrders
        {
            get => _processingOrders;
            set { _processingOrders = value; OnPropertyChanged(); }
        }

        public int CompletedOrders
        {
            get => _completedOrders;
            set { _completedOrders = value; OnPropertyChanged(); }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private readonly IServiceProvider _serviceProvider;

        public Orders()
        {
            InitializeComponent();
            _serviceProvider = App.ServiceProvider;
            Loaded += Orders_Loaded;
            DataContext = this;
        }

        private async void Orders_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

                // Load order counts
                var counts = await orderService.GetOrderStatusCountsAsync();
                TotalOrders = counts.Total;
                PendingOrders = counts.Pending;
                ProcessingOrders = counts.Processing;
                CompletedOrders = counts.Completed;

                var rows = await db.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Machine)
                    .Include(o => o.OrderEmployees)
                        .ThenInclude(oe => oe.Employee)
                    .Include(o => o.OrderMachines)
                    .Select(o => new OrderRow
                    {
                        OrderId = o.OrderId,
                        ProductId = o.ProductId,
                        ProductName = o.Product.ProductName,
                        Quantity = o.Quantity,
                        Status = o.Status.ToString(),
                        MachineName = o.Machine != null ? o.Machine.MachineName : "Chưa gán máy",
                        AssignedEmployees = string.Join(", ", o.OrderEmployees
                            .Where(oe => oe.Employee != null)
                            .Select(oe => oe.Employee.FullName)),
                        StartTime = o.OrderMachines.Select(om => (DateTime?)om.StartTime).FirstOrDefault(),
                        EndTime = o.OrderMachines.Select(om => om.EndTime).FirstOrDefault()
                    })
                    .OrderByDescending(r => r.OrderId)
                    .ToListAsync();

                OrdersDataGrid.ItemsSource = rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách đơn hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync();
        }

        private async void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            var win = new NewOrderWindow
            {
                Owner = Window.GetWindow(this)
            };
            win.Closed += async (_, __) => await LoadOrdersAsync();
            win.ShowDialog();
        }

        private async void AssignEmployees_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OrdersDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng chọn một Order trong danh sách.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Lấy OrderId từ item đang chọn (dùng reflection để an toàn)
                var selected = OrdersDataGrid.SelectedItem;
                var prop = selected.GetType().GetProperty("OrderId");
                if (prop == null)
                {
                    MessageBox.Show("Không xác định được OrderId từ dòng đã chọn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int orderId = (int)prop.GetValue(selected)!;

                var dlg = new AssignEmployeesWindow
                {
                    Owner = Window.GetWindow(this)
                };
                var ok = dlg.ShowDialog();
                if (ok != true) return;

                if (dlg.SelectedEmployeeIds == null || dlg.SelectedEmployeeIds.Length == 0)
                {
                    MessageBox.Show("Chưa chọn nhân viên nào.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var empService = scope.ServiceProvider.GetRequiredService<EmployeeService>();
                await empService.AssignEmployeesToOrderAsync(orderId, dlg.SelectedEmployeeIds, dlg.Role);

                MessageBox.Show("Phân công nhân viên thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi phân công nhân viên: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    
}
