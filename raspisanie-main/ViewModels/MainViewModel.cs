using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using raspisanie.Data;
using raspisanie.Models;
using raspisanie.Services;
using raspisanie.MVVM;

namespace raspisanie.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly AppDbContext _db;
    private readonly ScheduleService _scheduleService;
    private readonly HtmlScheduleParser _parser;
    private readonly IAttendanceService _attendanceService;

    private string _htmlFilePath = "raspisanie.html";
    private string _statusMessage = "Добро пожаловать в Трекер Посещаемости!";
    
    public MainViewModel()
    {
        _db = new AppDbContext();
        _db.Database.EnsureCreated();
        
        _scheduleService = new ScheduleService(_db);
        _parser = new HtmlScheduleParser();
        _attendanceService = new AttendanceService();
        
        LoadScheduleCommand = new RelayCommand(async () => await LoadScheduleAsync());
        RefreshCommand = new RelayCommand(() =>
        {
            RefreshData();
            StatusMessage = "Данные успешно обновлены из БД.";
        });
        
        RefreshData();
    }
    
    public string HtmlFilePath
    {
        get => _htmlFilePath;
        set => RaiseAndSetIfChanged(ref _htmlFilePath, value);
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
            var paras = _parser.Parse(HtmlFilePath);
            if (paras.Count == 0)
            {
                StatusMessage = "В файле не найдено занятий для загрузки.";
                return;
            }

            await _scheduleService.SaveAsync(paras);
            StatusMessage = $"Успешно загружено и сохранено {paras.Count} занятий.";
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
            var allModels = _db.Paras.OrderBy(p => p.Date).ThenBy(p => p.Time).ToList();
            
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
            _db.SaveChanges();
            
            // Re-fetch all models to compute correct stats
            var allModels = _db.Paras.ToList();
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
        AttendancePercentage = _attendanceService.CalculatePercentage(paras);
    }
}
