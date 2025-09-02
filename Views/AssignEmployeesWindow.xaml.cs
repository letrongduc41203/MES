using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MES.Services;
using MES.Models;

namespace MES.Views
{
    public partial class AssignEmployeesWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        public int[] SelectedEmployeeIds { get; private set; } = Array.Empty<int>();
        public string Role { get; private set; } = "Worker";

        public AssignEmployeesWindow()
        {
            InitializeComponent();
            _serviceProvider = App.ServiceProvider;
            Loaded += AssignEmployeesWindow_Loaded;
        }

        private async void AssignEmployeesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var empService = scope.ServiceProvider.GetRequiredService<EmployeeService>();
            var activeEmployees = await empService.GetActiveAsync();
            EmployeesListBox.ItemsSource = activeEmployees;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            SelectedEmployeeIds = EmployeesListBox.SelectedItems
                .OfType<Employee>()
                .Select(x => x.EmployeeId)
                .Distinct()
                .ToArray();
            Role = string.IsNullOrWhiteSpace(RoleTextBox.Text) ? "Worker" : RoleTextBox.Text.Trim();
            DialogResult = true;
        }
    }
}
