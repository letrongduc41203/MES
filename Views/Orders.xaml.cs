using System.Windows;
using System.Windows.Controls;
using MES.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MES.Models;

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
                    .Select(o => new OrderRow
                    {
                        OrderId = o.OrderId,
                        ProductId = o.ProductId,
                        ProductName = o.Product.ProductName,
                        Quantity = o.Quantity,
                        Status = o.Status.ToString(),
                        MachineName = o.Machine != null ? o.Machine.MachineName : "Chưa gán máy"
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
    }

    
}
