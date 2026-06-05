using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using raspisanie.Models;
using raspisanie.MVVM;
using raspisanie.MVVM.Memento;
using raspisanie.Services;

namespace raspisanie.ViewModels;

// ViewModel настроек.
// Слушает PropertyChanged → вызывает ScheduleSave (debounce).
// Пробрасывает IsSaving из SettingsService → View показывает spinner.
// Использует SettingsCaretaker для undo.
public class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService   _service   = SettingsService.Instance;
    private readonly SettingsCaretaker _caretaker = new();
    private readonly AppSettings       _settings;

    // Варианты вкладки по умолчанию — используются как ItemsSource в ComboBox
    public string[] TabOptions { get; } = ["Сегодня", "Все занятия", "Статистика"];

    // ── IsSaving ──────────────────────────────────────────
    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        private set => RaiseAndSetIfChanged(ref _isSaving, value);
    }

    public ICommand UndoCommand { get; }

    public SettingsViewModel(AppSettings settings)
    {
        _settings = settings;

        // Подписываемся на событие сервиса — обновляем IsSaving на UI-потоке
        _service.IsSavingChanged += () =>
            Dispatcher.UIThread.Post(() => IsSaving = _service.IsSaving);

        UndoCommand = new RelayCommand(Undo, () => _caretaker.CanUndo);
    }

    // ── Свойства-обёртки над AppSettings ─────────────────
    // Каждый сеттер:
    //  1. Делает снимок (Memento) перед изменением
    //  2. Применяет новое значение
    //  3. Вызывает ScheduleSave (debounce → async JSON write)

    public string HtmlFilePath
    {
        get => _settings.HtmlFilePath;
        set
        {
            if (_settings.HtmlFilePath == value) return;
            SaveSnapshot();                         // Memento: снимок до изменения
            _settings.HtmlFilePath = value;
            OnPropertyChanged();
            _service.ScheduleSave(_settings);       // debounce-сохранение
        }
    }

    public string DefaultTab
    {
        get => _settings.DefaultTab;
        set
        {
            if (_settings.DefaultTab == value) return;
            SaveSnapshot();
            _settings.DefaultTab = value;
            OnPropertyChanged();
            _service.ScheduleSave(_settings);
        }
    }

    // ── Memento helpers ───────────────────────────────────
    private void SaveSnapshot()
    {
        // Caretaker сохраняет снимок текущего состояния (до изменения)
        _caretaker.Push(_settings.CreateMemento());
    }

    private void Undo()
    {
        var memento = _caretaker.Pop();
        if (memento is null) return;

        // Originator восстанавливается из снимка
        _settings.Restore(memento);
        OnPropertyChanged(nameof(HtmlFilePath));
        OnPropertyChanged(nameof(DefaultTab));
        _service.ScheduleSave(_settings);           // сохраняем восстановлённое состояние
    }

    /// <summary>Обновляет настройки внутренней модели и уведомляет UI.</summary>
    public void UpdateSettings(AppSettings settings)
    {
        _settings.Restore(settings.CreateMemento());
        OnPropertyChanged(nameof(HtmlFilePath));
        OnPropertyChanged(nameof(DefaultTab));
    }

    // ── Async factory ─────────────────────────────────────
    /// <summary>Загружает настройки из JSON и возвращает готовый ViewModel.</summary>
    public static async Task<SettingsViewModel> CreateAsync()
    {
        var settings = await SettingsService.Instance.LoadAsync();
        return new SettingsViewModel(settings);
    }
}
