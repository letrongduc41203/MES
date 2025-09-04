using MES.Views;
using System.Windows;
using System.Windows.Controls;

namespace MES
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set initial content to Dashboard
            NavigateToView(btnDashboard);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                NavigateToView(button);
            }
        }

        private void NavigateToView(Button selectedButton)
        {
            // Reset all buttons to default style with null checks to avoid NRE
            if (this.Content is not Grid rootGrid || rootGrid.Children.Count == 0)
                return;

            var sidebarBorder = rootGrid.Children[0] as Border;
            var grid = sidebarBorder?.Child as Grid;
            var scrollViewer = grid != null && grid.Children.Count > 1 ? grid.Children[1] as ScrollViewer : null;
            var stackPanel = scrollViewer?.Content as StackPanel;

            if (stackPanel != null)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.Style = (Style)FindResource("MenuButtonStyle");
                    }
                }
            }

            // Set selected button to active style
            selectedButton.Style = (Style)FindResource("ActiveMenuButtonStyle");

            // Update page title and description
            if (txtPageTitle != null)
                txtPageTitle.Text = selectedButton.Content?.ToString() ?? "Dashboard";
            
            // Set appropriate description based on the page
            switch (selectedButton.Name)
            {
                case "btnDashboard":
                    if (txtPageDescription != null) txtPageDescription.Text = "Overview of PCB production activities";
                    MainContentControl.Content = new Dashboard();
                    break;
                case "btnOrders":
                    if (txtPageDescription != null) txtPageDescription.Text = "Manage production orders";
                    MainContentControl.Content = new Orders();
                    break;
                case "btnMaterials":
                    if (txtPageDescription != null) txtPageDescription.Text = "Manage materials and inventory";
                    MainContentControl.Content = new Materials();
                    break;
                case "btnMachines":
                    if (txtPageDescription != null) txtPageDescription.Text = "Manage machines and equipment";
                    MainContentControl.Content = new Machines();
                    break;
                case "btnEmployees":
                    if (txtPageDescription != null) txtPageDescription.Text = "Manage employee information";
                    MainContentControl.Content = new Employees();
                    break;
                case "btnReports":
                    if (txtPageDescription != null) txtPageDescription.Text = "View and generate reports";
                    MainContentControl.Content = new Reports();
                    break;
                case "btnSettings":
                    if (txtPageDescription != null) txtPageDescription.Text = "System configuration and settings";
                    MainContentControl.Content = new Settings();
                    break;
                default:
                    if (txtPageDescription != null) txtPageDescription.Text = "Overview of PCB production activities";
                    MainContentControl.Content = new Dashboard();
                    break;
            }
        }
    }
}