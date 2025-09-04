using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using MES.Models;


namespace MES.Views
{
    
    public partial class Materials : UserControl
    {
        public ObservableCollection<Material> MaterialList { get; set; } = new ObservableCollection<Material>();
        
        public Materials()
        {
            InitializeComponent();
            DataContext = this;
            LoadMaterials();
        }

        private void LoadMaterials()
        {
            MaterialList.Clear();

            string connectionString = "Server=DucFendi;Database=MES_ProductionDB;Trusted_Connection=True;TrustServerCertificate=True";
            string query = @"
            SELECT MaterialId, MaterialName, Unit, StockQuantity, LastUpdated
            FROM Materials
        ";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            MaterialList.Add(new Material
                            {
                                MaterialId = reader.GetInt32(0),
                                MaterialName = reader.GetString(1),
                                Unit = reader.GetString(2),
                                StockQuantity = reader.GetDouble(3),
                                LastUpdated = reader.GetDateTime(4)
                            });
                        }
                    }
                }

                // Update the DataGrid's ItemsSource
                materialDataGrid.ItemsSource = MaterialList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading materials: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic to handle Add Material button click
        }

        private void OrderMaterials_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic to handle Order Materials button click
        }

        private void ImportStock_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic to handle Import Stock button click
        }

        private void StockReport_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic to handle Stock Report button click
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic for search box focus
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic for search box lost focus
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Add logic for search box text changed
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Add logic for filter selection changed
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Add logic for tab selection changed
        }

        private void MaterialDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Add logic for material data grid selection changed
        }

        private void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic for edit material button click
        }

        private void OrderMaterial_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add logic for order material button click
        }
    }
}
