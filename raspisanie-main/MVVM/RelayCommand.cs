using System;
using System.Windows.Input;

namespace raspisanie.MVVM;

// ═══════════════════════════════════════════════════════
// ПАТТЕРН: КОМАНДА (Command) — GoF, поведенческий
// ═══════════════════════════════════════════════════════
// Оборачивает вызов метода в объект, реализующий ICommand.
// View (кнопка) не знает, что именно делает команда —
// она просто вызывает Execute(). Логика хранится в ViewModel.
// Это развязывает UI и бизнес-логику.
// ═══════════════════════════════════════════════════════
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
