using System.Windows.Input;

namespace MemoryLingo.Presentation.Commands;

public class ParameterizedRelayCommand<T> : ICommand
{
    readonly Action<T?> _execute;
    readonly Func<T?, bool>? _canExecute;

    public ParameterizedRelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
    public void Execute(object? parameter) => _execute((T?)parameter);
}
