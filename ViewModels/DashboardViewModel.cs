using MES.Data;
using MES.Models;
using MES.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MES.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly MachineService _machineService;
        private readonly MESDbContext _context;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Properties
        private int _totalMachines;
        private int _idleMachines;
        private int _runningMachines;
        private int _maintenanceMachines;
        private ObservableCollection<Machine> _recentMachines;

        public int TotalMachines
        {
            get => _totalMachines;
            set
            {
                _totalMachines = value;
                OnPropertyChanged(nameof(TotalMachines));
            }
        }

        public int IdleMachines
        {
            get => _idleMachines;
            set
            {
                _idleMachines = value;
                OnPropertyChanged(nameof(IdleMachines));
            }
        }

        public int RunningMachines
        {
            get => _runningMachines;
            set
            {
                _runningMachines = value;
                OnPropertyChanged(nameof(RunningMachines));
            }
        }

        public int MaintenanceMachines
        {
            get => _maintenanceMachines;
            set
            {
                _maintenanceMachines = value;
                OnPropertyChanged(nameof(MaintenanceMachines));
            }
        }

        public ObservableCollection<Machine> RecentMachines
        {
            get => _recentMachines;
            set
            {
                _recentMachines = value;
                OnPropertyChanged(nameof(RecentMachines));
            }
        }

        public DashboardViewModel(MachineService machineService, MESDbContext context)
        {
            _machineService = machineService;
            _context = context;
            _recentMachines = new ObservableCollection<Machine>();

            // Load dashboard data
            _ = LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var statistics = await _machineService.GetMachineStatisticsAsync();
                
                // Update machine counts
                TotalMachines = (int)statistics.GetType().GetProperty("Total")?.GetValue(statistics, null)!;
                IdleMachines = (int)statistics.GetType().GetProperty("Idle")?.GetValue(statistics, null)!;
                RunningMachines = (int)statistics.GetType().GetProperty("Running")?.GetValue(statistics, null)!;
                MaintenanceMachines = (int)statistics.GetType().GetProperty("Maintenance")?.GetValue(statistics, null)!;

                // Load recent machines
                var machines = await _machineService.GetAllMachinesAsync();
                var recentMachines = machines.Take(5).ToList(); // Get first 5 machines

                Application.Current.Dispatcher.Invoke(() =>
                {
                    RecentMachines.Clear();
                    foreach (var machine in recentMachines)
                    {
                        RecentMachines.Add(machine);
                    }
                });
            }
            catch (Exception ex)
            {
                // Log error or show message
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 