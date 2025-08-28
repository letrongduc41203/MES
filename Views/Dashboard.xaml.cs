using MES.Data;
using MES.Services;
using MES.ViewModels;
using System.Windows.Controls;

namespace MES.Views
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();
            
            // Initialize services and view model
            var context = new MESDbContext();
            var machineService = new MachineService(context);
            var viewModel = new DashboardViewModel(machineService, context);
            
            DataContext = viewModel;
        }
    }
}
