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
                var products = await dbContext.Products
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();

                ProductComboBox.ItemsSource = products;
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

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Số lượng phải là số nguyên dương.", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedDate = OrderDatePicker.SelectedDate ?? DateTime.Now;
                int productId = (int)ProductComboBox.SelectedValue;

                int newOrderId;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                    var order = new Order
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        OrderDate = selectedDate,
                        Status = OrderStatus.Pending
                    };

                    dbContext.Orders.Add(order);
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception exSaveOrder)
                    {
                        MessageBox.Show($"Lỗi khi lưu Orders: {ExtractError(exSaveOrder)}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var bomItems = await dbContext.ProductMaterials
                        .Where(pm => pm.ProductId == productId)
                        .ToListAsync();

                    foreach (var pm in bomItems)
                    {
                        var orderMaterial = new OrderMaterial
                        {
                            OrderId = order.OrderId,
                            MaterialId = pm.MaterialId,
                            QtyUsed = pm.QtyNeeded * quantity,
                            ProcessedQuantity = 0
                        };
                        dbContext.OrderMaterials.Add(orderMaterial);
                    }

                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception exSaveOM)
                    {
                        MessageBox.Show($"Lỗi khi lưu OrderMaterials: {ExtractError(exSaveOM)}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    newOrderId = order.OrderId;
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
                Console.WriteLine($"Waiting 10 seconds for Order {orderId}...");
                await Task.Delay(TimeSpan.FromSeconds(10));
                Console.WriteLine($"10 seconds passed for Order {orderId}");

                // Cập nhật status từ Pending -> Processing
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                    
                    var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                    Console.WriteLine($"Found order {orderId}: Status = {order?.Status}");
                    
                    if (order != null && order.Status == OrderStatus.Pending)
                    {
                        order.Status = OrderStatus.Processing;
                        await db.SaveChangesAsync();
                        Console.WriteLine($"Order {orderId} status updated to Processing successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Order {orderId} not found or status not Pending. Current status: {order?.Status}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating to Processing for Order {orderId}: {ExtractError(ex)}");
                }

                // Chờ 10 giây thứ hai
                Console.WriteLine($"Waiting another 10 seconds for Order {orderId}...");
                await Task.Delay(TimeSpan.FromSeconds(10));
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
