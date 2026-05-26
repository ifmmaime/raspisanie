using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using raspisanie.Models;
using raspisanie.Services;
using raspisanie.MVVM;

namespace raspisanie.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ScheduleFacade _facade;

    public SettingsViewModel Settings { get; }

    private string _statusMessage = "Добро пожаловать в Трекер Посещаемости!";
    private int _selectedTabIndex;
    
    public MainViewModel()
    {
        _facade = new ScheduleFacade();
        
        // Сначала создаем VM настроек с дефолтными значениями
        Settings = new SettingsViewModel(new AppSettings());

        // Запускаем асинхронную загрузку настроек из JSON
        _ = LoadSettingsAsync();

        // Подписываемся на синхронизацию путей к файлам
        Settings.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.HtmlFilePath))
            {
                OnPropertyChanged(nameof(HtmlFilePath));
            }
        };

        LoadScheduleCommand = new RelayCommand(async () => await LoadScheduleAsync());
        RefreshCommand = new RelayCommand(() =>
        {
            RefreshData();
            StatusMessage = "Данные успешно обновлены из БД.";
        });
        
        RefreshData();
    }

    private async Task LoadSettingsAsync()
    {
        var settingsModel = await SettingsService.Instance.LoadAsync();
        
        // Обновляем настройки на UI-потоке
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Settings.UpdateSettings(settingsModel);

            // Устанавливаем вкладку по умолчанию после загрузки настроек
            SelectedTabIndex = Settings.DefaultTab switch
            {
                "Сегодня" => 0,
                "Все занятия" => 1,
                "Статистика" => 2,
                "Настройки" => 3,
                _ => 0
            };
        });
    }
    
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set => RaiseAndSetIfChanged(ref _selectedTabIndex, value);
    }

    public string HtmlFilePath
    {
        get => Settings.HtmlFilePath;
        set
        {
            if (Settings.HtmlFilePath != value)
            {
                Settings.HtmlFilePath = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    public ObservableCollection<ParaViewModel> TodayParas { get; } = new();
    public ObservableCollection<ParaViewModel> AllParas { get; } = new();
    
    public bool HasTodayParas => TodayParas.Count > 0;
    public bool NoTodayParas => TodayParas.Count == 0;
    
    // Stats
    private int _totalLessons;
    public int TotalLessons
    {
        get => _totalLessons;
        set => RaiseAndSetIfChanged(ref _totalLessons, value);
    }
    
    private int _markedCount;
    public int MarkedCount
    {
        get => _markedCount;
        set
        {
            if (RaiseAndSetIfChanged(ref _markedCount, value))
            {
                OnPropertyChanged(nameof(AbsentCount));
            }
        }
    }
    
    private int _attendedCount;
    public int AttendedCount
    {
        get => _attendedCount;
        set
        {
            if (RaiseAndSetIfChanged(ref _attendedCount, value))
            {
                OnPropertyChanged(nameof(AbsentCount));
            }
        }
    }

    public int AbsentCount => MarkedCount - AttendedCount;
    
    private double _attendancePercentage;
    public double AttendancePercentage
    {
        get => _attendancePercentage;
        set
        {
            if (RaiseAndSetIfChanged(ref _attendancePercentage, value))
            {
                OnPropertyChanged(nameof(AttendancePercentageString));
            }
        }
    }
    
    public string AttendancePercentageString => $"{AttendancePercentage:F1}%";
    
    public ICommand LoadScheduleCommand { get; }
    public ICommand RefreshCommand { get; }
    
    private async Task LoadScheduleAsync()
    {
        if (string.IsNullOrWhiteSpace(HtmlFilePath))
        {
            StatusMessage = "Введите путь к HTML-файлу расписания.";
            return;
        }

        if (!File.Exists(HtmlFilePath))
        {
            StatusMessage = $"Файл не найден: {HtmlFilePath}";
            return;
        }
        
        try
        {
            StatusMessage = "Загрузка расписания...";
            var count = await _facade.LoadFromHtmlAsync(HtmlFilePath);
            if (count == 0)
            {
                StatusMessage = "В файле не найдено занятий для загрузки.";
                return;
            }

            StatusMessage = $"Успешно загружено и сохранено {count} занятий.";
            RefreshData();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }
    
    public void RefreshData()
    {
        try
        {
            // Get all paras ordered by Date and Time
            var allModels = _facade.GetAllLessons();
            
            var vms = allModels.Select(p => new ParaViewModel(p, OnParaChanged)).ToList();
            
            AllParas.Clear();
            foreach (var vm in vms)
            {
                AllParas.Add(vm);
            }
            
            TodayParas.Clear();
            var today = DateTime.Today;
            foreach (var vm in vms)
            {
                if (vm.Date.Date == today)
                {
                    TodayParas.Add(vm);
                }
            }
            
            UpdateStats(allModels);
            OnPropertyChanged(nameof(HasTodayParas));
            OnPropertyChanged(nameof(NoTodayParas));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки данных из БД: {ex.Message}";
        }
    }
    
    private void OnParaChanged()
    {
        try
        {
            _facade.SaveChanges();
            
            // Re-fetch all models to compute correct stats
            var allModels = _facade.GetRawList();
            UpdateStats(allModels);
            
            StatusMessage = "Изменения сохранены в базу данных.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }
    
    private void UpdateStats(List<Para> paras)
    {
        TotalLessons = paras.Count;
        MarkedCount = paras.Count(p => p.IsAttended != null);
        AttendedCount = paras.Count(p => p.IsAttended == true);
        AttendancePercentage = _facade.CalculateAttendancePercentage(paras);
    }
}
