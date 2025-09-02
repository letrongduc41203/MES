using MES.Data;
using MES.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MES.Services;

namespace MES.Views
{
    public partial class NewOrderWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public NewOrderWindow()
        {
            InitializeComponent();
            _serviceProvider = App.ServiceProvider;
            Loaded += NewOrderWindow_Loaded;
        }

        private static string ExtractError(Exception ex)
        {
            if (ex == null) return string.Empty;
            return string.Join(" -> ",
                new[]
                {
                    ex.Message,
                    ex.InnerException?.Message,
                    ex.InnerException?.InnerException?.Message
                }.Where(m => !string.IsNullOrWhiteSpace(m))
            );
        }

        private async void NewOrderWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var employeeService = scope.ServiceProvider.GetRequiredService<EmployeeService>();
                var products = await dbContext.Products
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();
                ProductComboBox.ItemsSource = products;

                var machines = await dbContext.Machines
                    .OrderBy(m => m.MachineName)
                    .ToListAsync();
                MachineComboBox.ItemsSource = machines;

                // Load active employees for assignment
                var activeEmployees = await employeeService.GetActiveAsync();
                EmployeesListBox.ItemsSource = activeEmployees;

                OrderDatePicker.SelectedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi nạp dữ liệu sản phẩm: {ExtractError(ex)}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if  (MachineComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Vui lòng chọn máy.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Số lượng phải là số nguyên dương.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedDate = OrderDatePicker.SelectedDate ?? DateTime.Now;
                int productId = (int)ProductComboBox.SelectedValue;
                int machineId = (int)MachineComboBox.SelectedValue;

                int newOrderId;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                    var newOrder = await orderService.CreateOrderAsync(productId, quantity, machineId, selectedDate);
                    newOrderId = newOrder.OrderId;

                    // Assign employees if any selected
                    var selectedEmployeeIds = EmployeesListBox.SelectedItems
                        .OfType<Employee>()
                        .Select(e => e.EmployeeId)
                        .ToList();
                    if (selectedEmployeeIds.Any())
                    {
                        var empService = scope.ServiceProvider.GetRequiredService<EmployeeService>();
                        var role = string.IsNullOrWhiteSpace(RoleTextBox.Text) ? "Worker" : RoleTextBox.Text.Trim();
                        await empService.AssignEmployeesToOrderAsync(newOrderId, selectedEmployeeIds, role);
                    }
                }

                // Debug: Hiển thị thông tin
                MessageBox.Show($"Tạo đơn hàng thành công. OrderId: {newOrderId}\nHệ thống sẽ tự động cập nhật trạng thái.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Chạy timer cập nhật status
                _ = Task.Run(async () => await RunOrderProgressionAsync(newOrderId));
                
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu đơn hàng: {ExtractError(ex)}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RunOrderProgressionAsync(int orderId)
        {
            try
            {
                Console.WriteLine($"Timer started for Order {orderId}");
                
                // Chờ 10 giây đầu tiên
                Console.WriteLine($"Waiting 20 seconds for Order {orderId}...");
                await Task.Delay(TimeSpan.FromSeconds(20));
                Console.WriteLine($"10 seconds passed for Order {orderId}");

                // Cập nhật status từ Pending -> Processing
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();
                    await orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Processing);
                    Console.WriteLine($"Order {orderId} status updated to Processing successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating to Processing for Order {orderId}: {ExtractError(ex)}");
                }

                // Chờ 10 giây thứ hai
                Console.WriteLine($"Waiting another 20 seconds for Order {orderId}...");
                await Task.Delay(TimeSpan.FromSeconds(20));
                Console.WriteLine($"20 seconds total passed for Order {orderId}");

                // Cập nhật status từ Processing -> Completed + trừ kho theo BOM
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<OrderService>();
                    await service.CompleteOrderAsync(orderId);
                    Console.WriteLine($"Order {orderId} status updated to Completed and materials deducted");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error completing order {orderId}: {ExtractError(ex)}");
                }
                
                Console.WriteLine($"Timer completed for Order {orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error in RunOrderProgressionAsync for Order {orderId}: {ExtractError(ex)}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
