using MES.Data;
using MES.Services;
using MES.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MES.Views
{
    public partial class Machines : UserControl
    {
        public Machines()
        {
            InitializeComponent();
            
            // Initialize services and view model
            var context = new MESDbContext();
            var machineService = new MachineService(context);
            var viewModel = new MachinesViewModel(machineService, context);
            
            DataContext = viewModel;
        }
    }
} 