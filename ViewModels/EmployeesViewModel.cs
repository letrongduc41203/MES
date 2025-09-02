using MES.Helpers;
using MES.Models;
using MES.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MES.ViewModels
{
    public class EmployeesViewModel : INotifyPropertyChanged
    {
        private readonly EmployeeService _service;

        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<Employee> _employees = new();
        private Employee? _selected;
        private string _newFullName = string.Empty;
        private string _newStatus = "Active";
        private bool _isLoading;

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set { _employees = value; OnPropertyChanged(nameof(Employees)); }
        }

        public Employee? SelectedEmployee
        {
            get => _selected;
            set { _selected = value; OnPropertyChanged(nameof(SelectedEmployee)); UpdateEditFieldsFromSelection(); _updateCommand.RaiseCanExecuteChanged(); _deleteCommand.RaiseCanExecuteChanged(); }
        }

        public string NewFullName
        {
            get => _newFullName;
            set { _newFullName = value; OnPropertyChanged(nameof(NewFullName)); _addCommand.RaiseCanExecuteChanged(); }
        }

        public string NewStatus
        {
            get => _newStatus;
            set { _newStatus = value; OnPropertyChanged(nameof(NewStatus)); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        // Commands
        public ICommand LoadCommand { get; }
        private readonly RelayCommand _addCommand;
        private readonly RelayCommand _updateCommand;
        private readonly RelayCommand _deleteCommand;
        public ICommand AddCommand => _addCommand;
        public ICommand UpdateCommand => _updateCommand;
        public ICommand DeleteCommand => _deleteCommand;
        public ICommand RefreshCommand { get; }

        public EmployeesViewModel(EmployeeService service)
        {
            _service = service;

            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            _addCommand = new RelayCommand(async _ => await AddAsync(), _ => !string.IsNullOrWhiteSpace(NewFullName));
            _updateCommand = new RelayCommand(async _ => await UpdateAsync(), _ => SelectedEmployee != null);
            _deleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedEmployee != null);

            _ = LoadAsync();
        }

        private void UpdateEditFieldsFromSelection()
        {
            if (SelectedEmployee != null)
            {
                NewFullName = SelectedEmployee.FullName;
                NewStatus = SelectedEmployee.Status;
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var items = await _service.GetAllAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Employees.Clear();
                    foreach (var e in items) Employees.Add(e);
                });
            }
            finally { IsLoading = false; }
        }

        private async Task AddAsync()
        {
            if (string.IsNullOrWhiteSpace(NewFullName))
            {
                MessageBox.Show("Vui lòng nhập tên nhân viên.");
                return;
            }
            var emp = await _service.AddEmployeeAsync(NewFullName.Trim());
            Employees.Add(emp);
            SelectedEmployee = emp;
            MessageBox.Show("Thêm nhân viên thành công.");
        }

        private async Task UpdateAsync()
        {
            if (SelectedEmployee == null) return;
            await _service.UpdateEmployeeAsync(SelectedEmployee.EmployeeId, NewFullName, NewStatus);
            await LoadAsync();
            MessageBox.Show("Cập nhật nhân viên thành công.");
        }

        private async Task DeleteAsync()
        {
            if (SelectedEmployee == null) return;
            var ok = await _service.DeleteEmployeeAsync(SelectedEmployee.EmployeeId);
            if (ok)
            {
                await LoadAsync();
                MessageBox.Show("Xóa nhân viên (mềm nếu có liên kết) thành công.");
            }
        }

        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
