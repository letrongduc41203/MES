using MES.Helpers;
using MES.Models;
using MES.Services;
using MES.Data;
using MES.Views; // Add this using
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
                OnPropertyChanged(nameof(CanStartMaintenance));
                OnPropertyChanged(nameof(CanCompleteMaintenance));
                // Explicitly update command state
                ((RelayCommand)AddMaintenanceCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateStatusCommand).RaiseCanExecuteChanged();
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
        public bool CanStartMaintenance => SelectedMachine != null && SelectedMachine.Status != MachineStatus.Maintenance && SelectedMachine.Status != MachineStatus.Running;
        public bool CanCompleteMaintenance => SelectedMachine != null && SelectedMachine.Status == MachineStatus.Maintenance;

        // Commands
        public ICommand LoadMachinesCommand { get; }
        public ICommand AddMachineCommand { get; }
        public ICommand EditMachineCommand { get; }
        public ICommand DeleteMachineCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand AddMaintenanceCommand { get; }
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
            UpdateStatusCommand = new RelayCommand(async _ => await CompleteMaintenanceAsync(), _ => CanCompleteMaintenance); // Renamed and added CanExecute
            AddMaintenanceCommand = new RelayCommand(_ => StartMaintenanceAsyncSafe(), _ => CanStartMaintenance); // Renamed and added CanExecute
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

        private async Task StartMaintenanceAsync()
        {
            if (!CanStartMaintenance) return;

            var dialog = new MaintenanceInputDialog();

            // Only set Owner if MainWindow is shown
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
            {
                dialog.Owner = Application.Current.MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                var description = dialog.Description;
                var technician = dialog.Technician;

                if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(technician))
                {
                    MessageBox.Show("Mô tả và tên kỹ thuật viên không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    var success = await _machineService.StartMaintenanceAsync(SelectedMachine!.MachineId, description, technician);
                    if (success)
                    {
                        MessageBox.Show("Bắt đầu bảo trì máy thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadMachinesAsync(); // Refresh list
                    }
                    else
                    {
                        MessageBox.Show("Không thể bắt đầu bảo trì. Máy có thể đang chạy hoặc không tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi bắt đầu bảo trì: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task CompleteMaintenanceAsync()
        {
            if (!CanCompleteMaintenance) return;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn hoàn tất bảo trì cho máy '{SelectedMachine!.MachineName}'?",
                "Xác nhận hoàn tất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            { 
                try
                {
                    var success = await _machineService.CompleteMaintenanceAsync(SelectedMachine!.MachineId);
                    if (success)
                    {
                        MessageBox.Show("Hoàn tất bảo trì thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadMachinesAsync(); // Refresh list
                    }
                    else
                    {
                        MessageBox.Show("Không thể hoàn tất bảo trì. Máy không ở trạng thái bảo trì.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi hoàn tất bảo trì: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private void StartMaintenanceAsyncSafe()
        {
            // Call the Task and handle exceptions explicitly so we don't rely on async void with unobserved exceptions
            var task = StartMaintenanceAsync();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    // marshal back to UI thread to show message
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Unhandled error when starting maintenance: {t.Exception.GetBaseException().Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }, TaskScheduler.Default);
        }
    }
}