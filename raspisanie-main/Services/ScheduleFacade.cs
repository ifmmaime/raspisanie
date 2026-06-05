using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using raspisanie.Data;
using raspisanie.Models;

namespace raspisanie.Services;

// ═══════════════════════════════════════════════════════
// ПАТТЕРН: ФАСАД (Facade) — GoF, структурный
// ═══════════════════════════════════════════════════════
// Скрывает сложность подсистемы (база данных, парсер HTML,
// сервисы) за единым простым интерфейсом.
// MainViewModel не знает про AppDbContext, HtmlScheduleParser
// и т.д. — он вызывает только методы ScheduleFacade.
// ═══════════════════════════════════════════════════════
public class ScheduleFacade
{
    private readonly AppDbContext _db;
    private readonly ScheduleService _scheduleService;
    private readonly HtmlScheduleParser _parser;
    private readonly IAttendanceService _attendanceService;

    public ScheduleFacade()
    {
        _db = new AppDbContext();
        _db.Database.EnsureCreated();
        
        _scheduleService = new ScheduleService(_db);
        _parser = new HtmlScheduleParser();
        _attendanceService = AttendanceService.Instance; // Singleton pattern usage
    }

    public async Task<int> LoadFromHtmlAsync(string filePath)
    {
        var paras = _parser.Parse(filePath);
        await _scheduleService.SaveAsync(paras);
        return paras.Count;
    }

    public List<Para> GetAllLessons()
    {
        return _db.Paras.OrderBy(p => p.Date).ThenBy(p => p.Time).ToList();
    }

    public List<Para> GetTodayLessons()
    {
        var today = DateTime.Today;
        return _db.Paras
            .Where(p => p.Date.Date == today)
            .OrderBy(p => p.Time)
            .ToList();
    }

    public double CalculateAttendancePercentage(List<Para> paras)
    {
        return _attendanceService.CalculatePercentage(paras);
    }

    public void SaveChanges()
    {
        _db.SaveChanges();
    }

    public List<Para> GetRawList()
    {
        return _db.Paras.ToList();
    }
}
