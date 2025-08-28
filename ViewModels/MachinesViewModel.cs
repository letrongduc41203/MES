using MES.Helpers;
using MES.Models;
using MES.Services;
using MES.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MES.ViewModels
{
    public class MachinesViewModel : INotifyPropertyChanged
    {
        private readonly MachineService _machineService;
        private readonly MESDbContext _context;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Properties
        private ObservableCollection<Machine> _machines;
        private Machine? _selectedMachine;
        private string _searchTerm = string.Empty;
        private MachineStatus _selectedStatus = MachineStatus.Idle;
        private bool _isLoading = false;

        public ObservableCollection<Machine> Machines
        {
            get => _machines;
            set
            {
                _machines = value;
                OnPropertyChanged(nameof(Machines));
            }
        }

        public Machine? SelectedMachine
        {
            get => _selectedMachine;
            set
            {
                _selectedMachine = value;
                OnPropertyChanged(nameof(SelectedMachine));
                OnPropertyChanged(nameof(CanEditMachine));
                OnPropertyChanged(nameof(CanDeleteMachine));
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
                _ = SearchMachinesAsync();
            }
        }

        public MachineStatus SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
                _ = FilterMachinesByStatusAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool CanEditMachine => SelectedMachine != null;
        public bool CanDeleteMachine => SelectedMachine != null && SelectedMachine.Status != MachineStatus.Running;

        // Commands
        public ICommand LoadMachinesCommand { get; }
        public ICommand AddMachineCommand { get; }
        public ICommand EditMachineCommand { get; }
        public ICommand DeleteMachineCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand AddMaintenanceCommand { get; }
        public ICommand AssignOrderCommand { get; }
        public ICommand CompleteOrderCommand { get; }
        public ICommand RefreshCommand { get; }

        public MachinesViewModel(MachineService machineService, MESDbContext context)
        {
            _machineService = machineService;
            _context = context;
            _machines = new ObservableCollection<Machine>();

            // Initialize Commands
            LoadMachinesCommand = new RelayCommand(async _ => await LoadMachinesAsync());
            AddMachineCommand = new RelayCommand(_ => AddMachine());
            EditMachineCommand = new RelayCommand(_ => EditMachine(), _ => CanEditMachine);
            DeleteMachineCommand = new RelayCommand(async _ => await DeleteMachineAsync(), _ => CanDeleteMachine);
            UpdateStatusCommand = new RelayCommand(async _ => await UpdateMachineStatusAsync());
            AddMaintenanceCommand = new RelayCommand(_ => AddMaintenance());
            AssignOrderCommand = new RelayCommand(_ => AssignOrder());
            CompleteOrderCommand = new RelayCommand(_ => CompleteOrder());
            RefreshCommand = new RelayCommand(async _ => await LoadMachinesAsync());

            // Load machines on initialization
            _ = LoadMachinesAsync();
        }

        private async Task LoadMachinesAsync()
        {
            try
            {
                IsLoading = true;
                var machines = await _machineService.GetAllMachinesAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Machines.Clear();
                    foreach (var machine in machines)
                    {
                        Machines.Add(machine);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách máy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchMachinesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                await LoadMachinesAsync();
                return;
            }

            try
            {
                var machines = await _machineService.SearchMachinesAsync(SearchTerm);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Machines.Clear();
                    foreach (var machine in machines)
                    {
                        Machines.Add(machine);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task FilterMachinesByStatusAsync()
        {
            try
            {
                var machines = await _machineService.GetMachinesByStatusAsync(SelectedStatus);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Machines.Clear();
                    foreach (var machine in machines)
                    {
                        Machines.Add(machine);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lọc máy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMachine()
        {
            // TODO: Open Add Machine Window
            MessageBox.Show("Chức năng thêm máy sẽ được triển khai", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditMachine()
        {
            if (SelectedMachine == null) return;
            
            // TODO: Open Edit Machine Window
            MessageBox.Show($"Chỉnh sửa máy: {SelectedMachine.MachineName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task DeleteMachineAsync()
        {
            if (SelectedMachine == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa máy '{SelectedMachine.MachineName}'?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _machineService.DeleteMachineAsync(SelectedMachine.MachineId);
                    if (success)
                    {
                        Machines.Remove(SelectedMachine);
                        SelectedMachine = null;
                        MessageBox.Show("Xóa máy thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa máy đang hoạt động", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa máy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task UpdateMachineStatusAsync()
        {
            if (SelectedMachine == null) return;

            // TODO: Open Status Update Window
            MessageBox.Show($"Cập nhật trạng thái máy: {SelectedMachine.MachineName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AddMaintenance()
        {
            if (SelectedMachine == null) return;

            // TODO: Open Maintenance Window
            MessageBox.Show($"Thêm bảo trì cho máy: {SelectedMachine.MachineName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AssignOrder()
        {
            if (SelectedMachine == null) return;

            // TODO: Open Order Assignment Window
            MessageBox.Show($"Gán đơn hàng cho máy: {SelectedMachine.MachineName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CompleteOrder()
        {
            if (SelectedMachine == null) return;

            // TODO: Open Order Completion Window
            MessageBox.Show($"Hoàn thành đơn hàng trên máy: {SelectedMachine.MachineName}", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 