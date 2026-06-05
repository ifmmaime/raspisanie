using System.Collections.Generic;
using raspisanie.Models;

namespace raspisanie.Services;

// ═══════════════════════════════════════════════════════
// ПАТТЕРН: ОДИНОЧКА (Singleton) — GoF, порождающий
// ═══════════════════════════════════════════════════════
// Гарантирует, что класс имеет ровно один экземпляр
// на всё приложение. Конструктор закрыт — создать объект
// снаружи нельзя; доступ только через AttendanceService.Instance.
// Блокировка (_lock) делает создание потокобезопасным.
// ═══════════════════════════════════════════════════════
public class AttendanceService : IAttendanceService // считает процент посещённых пар
{
    private static AttendanceService? _instance;
    private static readonly object _lock = new object();

    private AttendanceService() { } // закрытый конструктор — запрещает создание извне (часть Singleton)

    public static AttendanceService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new AttendanceService();
                }
            }
            return _instance;
        }
    }

    public double CalculatePercentage(List<Para> paras)
    {
        var total    = paras.Count(p => p.IsAttended != null);
        var attended = paras.Count(p => p.IsAttended == true);

        if (total == 0) return 0;

        return (double)attended / total * 100;
    }
}
