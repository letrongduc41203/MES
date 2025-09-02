using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MES.Services;
using MES.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MES.Views
{
    public partial class MaintenanceInputDialog : Window
    {
        public string Description { get; private set; } = string.Empty;
        public string Technician { get; private set; } = string.Empty;
        private readonly EmployeeService _employeeService;

        public MaintenanceInputDialog()
        {
            InitializeComponent();
            var sp = App.ServiceProvider;
            if (sp != null)
            {
                _employeeService = sp.GetRequiredService<EmployeeService>();
            }
            Loaded += MaintenanceInputDialog_Loaded;
        }

        private async void MaintenanceInputDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (_employeeService == null) return;
            var employees = await _employeeService.GetActiveAsync();
            TechnicianComboBox.ItemsSource = employees;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Description = DescriptionTextBox.Text;
            if (TechnicianComboBox.SelectedValue is int empId)
            {
                Technician = empId.ToString();
            }
            else
            {
                Technician = string.Empty;
            }
            DialogResult = true;
        }
    }
}
