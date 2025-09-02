using MES.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json; // Add this using directive
using System.IO;
using Microsoft.EntityFrameworkCore;
using MES.Data;
using MES.Services;


namespace MES
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            
            serviceCollection.AddDbContext<MyDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Đăng ký OrderService
            serviceCollection.AddScoped<OrderService>();
            serviceCollection.AddScoped<EmployeeService>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

            base.OnStartup(e);

            var loginWindow = new Login();
            loginWindow.Show();
        }
    }
}





