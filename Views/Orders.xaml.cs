using System.Windows;
using System.Windows.Controls;
using System.Linq;
using MES.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MES.Models;
using MES.Services;

namespace MES.Views
{
    /// <summary>
    /// Interaction logic for Orders.xaml
    /// </summary>
    public partial class Orders : UserControl
    {
        private readonly IServiceProvider _serviceProvider;

        public Orders()
        {
            InitializeComponent();
            _serviceProvider = App.ServiceProvider;
            Loaded += Orders_Loaded;
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

                var rows = await db.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Machine)
                    .Include(o => o.OrderEmployees)
                        .ThenInclude(oe => oe.Employee)
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
                            .Select(oe => oe.Employee.FullName))
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
