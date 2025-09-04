using MES.Data;
using MES.Models;
using MES.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace MES.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly MachineService _machineService;
        private readonly MyDbContext _db; // use main DI db for queries

        public event PropertyChangedEventHandler? PropertyChanged;

        // Time range selection (bound to RadioButtons)
        private bool _isTodaySelected = true;
        private bool _isWeekSelected;
        private bool _isMonthSelected;

        public bool IsTodaySelected
        {
            get => _isTodaySelected;
            set
            {
                if (_isTodaySelected != value)
                {
                    _isTodaySelected = value;
                    OnPropertyChanged(nameof(IsTodaySelected));
                    if (value) { _isWeekSelected = false; _isMonthSelected = false; OnPropertyChanged(nameof(IsWeekSelected)); OnPropertyChanged(nameof(IsMonthSelected)); _ = LoadDashboardDataAsync(); }
                }
            }
        }

        public bool IsWeekSelected
        {
            get => _isWeekSelected;
            set
            {
                if (_isWeekSelected != value)
                {
                    _isWeekSelected = value;
                    OnPropertyChanged(nameof(IsWeekSelected));
                    if (value) { _isTodaySelected = false; _isMonthSelected = false; OnPropertyChanged(nameof(IsTodaySelected)); OnPropertyChanged(nameof(IsMonthSelected)); _ = LoadDashboardDataAsync(); }
                }
            }
        }

        public bool IsMonthSelected
        {
            get => _isMonthSelected;
            set
            {
                if (_isMonthSelected != value)
                {
                    _isMonthSelected = value;
                    OnPropertyChanged(nameof(IsMonthSelected));
                    if (value) { _isTodaySelected = false; _isWeekSelected = false; OnPropertyChanged(nameof(IsTodaySelected)); OnPropertyChanged(nameof(IsWeekSelected)); _ = LoadDashboardDataAsync(); }
                }
            }
        }

        // Orders Overview
        private int _totalOrders;
        private int _inProgressOrders;
        private int _completedOrders;

        public int TotalOrders { get => _totalOrders; set { _totalOrders = value; OnPropertyChanged(nameof(TotalOrders)); } }
        public int InProgressOrders { get => _inProgressOrders; set { _inProgressOrders = value; OnPropertyChanged(nameof(InProgressOrders)); } }
        public int CompletedOrders { get => _completedOrders; set { _completedOrders = value; OnPropertyChanged(nameof(CompletedOrders)); } }

        // Machines Status mapped to UI bindings
        private int _availableMachines;
        private int _inUseMachines;
        private int _maintenanceMachines;
        private int _totalMachines;

        public int AvailableMachines { get => _availableMachines; set { _availableMachines = value; OnPropertyChanged(nameof(AvailableMachines)); } }
        public int InUseMachines { get => _inUseMachines; set { _inUseMachines = value; OnPropertyChanged(nameof(InUseMachines)); } }
        public int MaintenanceMachines { get => _maintenanceMachines; set { _maintenanceMachines = value; OnPropertyChanged(nameof(MaintenanceMachines)); } }
        public int TotalMachines { get => _totalMachines; set { _totalMachines = value; OnPropertyChanged(nameof(TotalMachines)); } }

        // Employees Activity
        private int _activeEmployees;
        private int _productionEmployees;
        public int ActiveEmployees { get => _activeEmployees; set { _activeEmployees = value; OnPropertyChanged(nameof(ActiveEmployees)); } }
        public int ProductionEmployees { get => _productionEmployees; set { _productionEmployees = value; OnPropertyChanged(nameof(ProductionEmployees)); } }

        public class TopEmployeeRow
        {
            public int Rank { get; set; }
            public string EmployeeName { get; set; } = string.Empty;
            public int OrderCount { get; set; }
        }

        private ObservableCollection<TopEmployeeRow> _topEmployees = new();
        public ObservableCollection<TopEmployeeRow> TopEmployees
        {
            get => _topEmployees;
            set { _topEmployees = value; OnPropertyChanged(nameof(TopEmployees)); }
        }

        // Production Output
        private int _totalQuantityProduced;
        private double _averageQuantityPerOrder;
        public int TotalQuantityProduced { get => _totalQuantityProduced; set { _totalQuantityProduced = value; OnPropertyChanged(nameof(TotalQuantityProduced)); } }
        public double AverageQuantityPerOrder { get => _averageQuantityPerOrder; set { _averageQuantityPerOrder = value; OnPropertyChanged(nameof(AverageQuantityPerOrder)); } }

        // Maintenance Summary
        private int _maintenanceCount;
        private string _topMaintenanceMachine = string.Empty;
        private int _topMaintenanceMachineCount;
        private string _topTechnician = string.Empty;
        private int _topTechnicianCount;

        public int MaintenanceCount { get => _maintenanceCount; set { _maintenanceCount = value; OnPropertyChanged(nameof(MaintenanceCount)); } }
        public string TopMaintenanceMachine { get => _topMaintenanceMachine; set { _topMaintenanceMachine = value; OnPropertyChanged(nameof(TopMaintenanceMachine)); } }
        public int TopMaintenanceMachineCount { get => _topMaintenanceMachineCount; set { _topMaintenanceMachineCount = value; OnPropertyChanged(nameof(TopMaintenanceMachineCount)); } }
        public string TopTechnician { get => _topTechnician; set { _topTechnician = value; OnPropertyChanged(nameof(TopTechnician)); } }
        public int TopTechnicianCount { get => _topTechnicianCount; set { _topTechnicianCount = value; OnPropertyChanged(nameof(TopTechnicianCount)); } }

        // Recent machines (optional use in future UI)
        private ObservableCollection<Machine> _recentMachines = new();
        public ObservableCollection<Machine> RecentMachines
        {
            get => _recentMachines;
            set { _recentMachines = value; OnPropertyChanged(nameof(RecentMachines)); }
        }

        private readonly DispatcherTimer _refreshTimer;

        public DashboardViewModel(MachineService machineService, MESDbContext context)
        {
            _machineService = machineService;
            // Accept MESDbContext param to keep signature from code-behind, but use MyDbContext via App.ServiceProvider if available
            // Try to get MyDbContext from App.ServiceProvider; fall back to creating one from MESDbContext connection is out of scope, so only use if available.
          _db = App.ServiceProvider.GetRequiredService<MyDbContext>();


            // Load dashboard data
            _ = LoadDashboardDataAsync();

            // Auto refresh every 30 seconds
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _refreshTimer.Tick += async (_, __) =>
            {
                await LoadDashboardDataAsync();
            };
            _refreshTimer.Start();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // 1) Determine time range
                DateTime fromDate;
                var now = DateTime.Now;
                if (IsTodaySelected)
                {
                    fromDate = now.Date;
                }
                else if (IsWeekSelected)
                {
                    // start of week (Mon)
                    int diff = (7 + (int)now.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                    fromDate = now.Date.AddDays(-diff);
                }
                else // Month
                {
                    fromDate = new DateTime(now.Year, now.Month, 1);
                }

                // 2) Machines stats
                var statistics = await _machineService.GetMachineStatisticsAsync();
                TotalMachines = (int)statistics.GetType().GetProperty("Total")?.GetValue(statistics, null)!;
                var running = (int)statistics.GetType().GetProperty("Running")?.GetValue(statistics, null)!;
                var idle = (int)statistics.GetType().GetProperty("Idle")?.GetValue(statistics, null)!;
                var maintenance = (int)statistics.GetType().GetProperty("Maintenance")?.GetValue(statistics, null)!;
                var available = await _db.Machines.CountAsync(m => m.Status == MachineStatus.Available);
                var busy = await _db.Machines.CountAsync(m => m.Status == MachineStatus.Busy);
                AvailableMachines = available;
                InUseMachines = running + busy;
                MaintenanceMachines = maintenance;

                // 3) Orders metrics within time range
                var ordersQuery = _db.Orders.Where(o => o.OrderDate >= fromDate);
                TotalOrders = await ordersQuery.CountAsync();
                InProgressOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Processing);
                CompletedOrders = await ordersQuery.CountAsync(o => o.Status == OrderStatus.Completed);

                // 4) Employees activity
                ActiveEmployees = await _db.Employees.CountAsync(e => e.Status == "Active");
                // ProductionEmployees: unique employees assigned to in-progress orders in range
                var prodEmp = await (from oe in _db.OrderEmployees
                                     join o in _db.Orders on oe.OrderId equals o.OrderId
                                     where o.OrderDate >= fromDate && o.Status != OrderStatus.Pending
                                     select oe.EmployeeId).Distinct().CountAsync();
                ProductionEmployees = prodEmp;

                // Top Employees by number of orders assigned in range
                var topEmp = await (from oe in _db.OrderEmployees
                                    join o in _db.Orders on oe.OrderId equals o.OrderId
                                    join e in _db.Employees on oe.EmployeeId equals e.EmployeeId
                                    where o.OrderDate >= fromDate
                                    group new { oe, e } by new { e.EmployeeId, e.FullName } into g
                                    orderby g.Count() descending, g.Key.FullName
                                    select new { g.Key.FullName, C = g.Count() })
                                   .Take(5)
                                   .ToListAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TopEmployees.Clear();
                    int rank = 1;
                    foreach (var row in topEmp)
                    {
                        TopEmployees.Add(new TopEmployeeRow { Rank = rank++, EmployeeName = row.FullName, OrderCount = row.C });
                    }
                });

                // 5) Production output (Completed orders in range)
                var completedOrders = await ordersQuery.Where(o => o.Status == OrderStatus.Completed).ToListAsync();
                TotalQuantityProduced = completedOrders.Sum(o => o.Quantity);
                AverageQuantityPerOrder = completedOrders.Count > 0 ? Math.Round(completedOrders.Average(o => o.Quantity), 2) : 0;

                // 6) Maintenance summary within time range
                var maintQuery = _db.MachineMaintenances.Where(mm => mm.MaintenanceDate >= fromDate);
                MaintenanceCount = await maintQuery.CountAsync();

                var topMachine = await (from mm in maintQuery
                                        join m in _db.Machines on mm.MachineId equals m.MachineId
                                        group mm by new { m.MachineId, m.MachineName } into g
                                        orderby g.Count() descending, g.Key.MachineName
                                        select new { g.Key.MachineName, C = g.Count() })
                                       .FirstOrDefaultAsync();
                TopMaintenanceMachine = topMachine?.MachineName ?? "-";
                TopMaintenanceMachineCount = topMachine?.C ?? 0;

                var topTech = await maintQuery
                                    .GroupBy(mm => mm.Technician)
                                    .Select(g => new { Technician = g.Key, C = g.Count() })
                                    .OrderByDescending(x => x.C)
                                    .FirstOrDefaultAsync();
                if (topTech != null && int.TryParse(topTech.Technician, out int techEmpId))
                {
                    var empName = await _db.Employees.Where(e => e.EmployeeId == techEmpId).Select(e => e.FullName).FirstOrDefaultAsync();
                    TopTechnician = empName ?? topTech.Technician;
                }
                else
                {
                    TopTechnician = topTech?.Technician ?? "-";
                }
                TopTechnicianCount = topTech?.C ?? 0;

                // 7) Recent machines (optional)
                var machines = await _machineService.GetAllMachinesAsync();
                var recentMachines = machines.Take(5).ToList();
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