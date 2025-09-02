using System;
using System.Windows.Input;

namespace MES.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        private event EventHandler? _canExecuteChangedInternal;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                _canExecuteChangedInternal += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                _canExecuteChangedInternal -= value;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            _canExecuteChangedInternal?.Invoke(this, EventArgs.Empty);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
