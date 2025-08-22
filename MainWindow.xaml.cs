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
            // Reset all buttons to default style
            var sidebarStack = ((Grid)this.Content).Children[0] as Border;
            var grid = sidebarStack.Child as Grid;
            var scrollViewer = ((Grid)grid).Children[1] as ScrollViewer;
            var stackPanel = scrollViewer.Content as StackPanel;
            
            foreach (var child in stackPanel.Children)
            {
                if (child is Button btn)
                {
                    btn.Style = (Style)FindResource("MenuButtonStyle");
                }
            }

            // Set selected button to active style
            selectedButton.Style = (Style)FindResource("ActiveMenuButtonStyle");

            // Update page title and description
            txtPageTitle.Text = selectedButton.Content?.ToString() ?? "Dashboard";
            
            // Set appropriate description based on the page
            switch (selectedButton.Name)
            {
                case "btnDashboard":
                    txtPageDescription.Text = "Overview of PCB production activities";
                    MainContentControl.Content = new Dashboard();
                    break;
                case "btnOrders":
                    txtPageDescription.Text = "Manage production orders";
                    MainContentControl.Content = new Orders();
                    break;
                case "btnMaterials":
                    txtPageDescription.Text = "Manage materials and inventory";
                    MainContentControl.Content = new Materials();
                    break;
                case "btnEmployees":
                    txtPageDescription.Text = "Manage employee information";
                    MainContentControl.Content = new Employees();
                    break;
                case "btnReports":
                    txtPageDescription.Text = "View and generate reports";
                    MainContentControl.Content = new Reports();
                    break;
                case "btnSettings":
                    txtPageDescription.Text = "System configuration and settings";
                    MainContentControl.Content = new Settings();
                    break;
                default:
                    txtPageDescription.Text = "Overview of PCB production activities";
                    MainContentControl.Content = new Dashboard();
                    break;
            }
        }
    }
}