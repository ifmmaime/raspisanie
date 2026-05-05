using raspisanie.Data;
using raspisanie.Models;
using raspisanie.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding  = System.Text.Encoding.UTF8;

using var db = new AppDbContext();
db.Database.EnsureCreated();

var scheduleService   = new ScheduleService(db);
var parser            = new HtmlScheduleParser();
var attendanceService = new AttendanceService();

Console.WriteLine("=== Трекер посещаемости ===\n");

bool running = true;
while (running)
{
    Console.WriteLine("1. Загрузить расписание из HTML файла");
    Console.WriteLine("2. Занятия на сегодня");
    Console.WriteLine("3. Отметить посещение");
    Console.WriteLine("4. Статистика посещаемости");
    Console.WriteLine("5. Выход");
    Console.Write("\nВыберите пункт: ");

    var choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (choice)
    {
        case "1": await LoadFromHtml(parser, scheduleService); break;
        case "2": ShowToday(db); break;
        case "3": MarkAttendance(db); break;
        case "4": ShowStats(db, attendanceService); break;
        case "5": running = false; Console.WriteLine("До свидания!"); break;
        default:  Console.WriteLine("Неверный выбор.\n"); break;
    }
}


static async Task LoadFromHtml(HtmlScheduleParser parser, ScheduleService service) // по кнопке 1 полная загрузка в бд
{
    Console.WriteLine("Введите путь к HTML файлу (или Enter для raspisanie.html):");
    Console.Write("> ");

    var input = Console.ReadLine()?.Trim().Trim('"') ?? "";
    var path  = input.Length > 0 ? input : "raspisanie.html";

    if (!File.Exists(path))
    {
        Console.WriteLine($"Файл не найден: {path}\n");
        return;
    }

    try
    {
        var paras = parser.Parse(path);
        await service.SaveAsync(paras);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }

    Console.WriteLine();
}

static void ShowToday(AppDbContext db) // Занятия которые седня по кнопке 2
{
    var today = DateTime.Today;
    var paras = db.Paras
        .Where(p => p.Date.Date == today)
        .OrderBy(p => p.Time)
        .ToList();

    if (paras.Count == 0)
    {
        Console.WriteLine($"На сегодня ({today:dd.MM.yyyy}) занятий нет.\n");
        return;
    }

    Console.WriteLine($"Занятия на {today:dd.MM.yyyy}:");
    foreach (var p in paras)
    {
        var status = p.IsAttended switch { true => "✔ Был", false => "✖ Не был", null => "— Не отмечено" };
        Console.WriteLine($"  [{p.Id}] {p.Time}  {p.Subject}  ({p.Teacher}, ауд. {p.Room})  {status}");
    }
    Console.WriteLine();
}


static void MarkAttendance(AppDbContext db)  // по кнопке 3 отметить посещение пары (выбрать по id)
{
    var paras = db.Paras.OrderBy(p => p.Date).ThenBy(p => p.Time).ToList();

    if (paras.Count == 0)
    {
        Console.WriteLine("Расписание пусто. Сначала загрузите HTML (пункт 1).\n");
        return;
    }

    Console.WriteLine("Все занятия:");
    foreach (var p in paras)
    {
        var status = p.IsAttended switch { true => "✔", false => "✖", null => "—" };
        Console.WriteLine($"  [{p.Id}] {p.Date:dd.MM.yyyy} {p.Time}  {p.Subject}  {status}");
    }

    Console.Write("\nВведите ID пары: ");
    if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID.\n"); return; }

    var para = db.Paras.Find(id);
    if (para == null) { Console.WriteLine("Пара не найдена.\n"); return; }

    Console.Write("Посетили? (1 = да / 0 = нет): ");
    para.IsAttended = Console.ReadLine()?.Trim() == "1";
    db.SaveChanges();

    Console.WriteLine($"Отмечено: {(para.IsAttended == true ? "✔ Был" : "✖ Не был")}\n");
}


static void ShowStats(AppDbContext db, IAttendanceService service) // по кнопке вся 4 статистика посещаемости
{
    var paras = db.Paras.ToList();
    if (paras.Count == 0) { Console.WriteLine("Нет данных для статистики.\n"); return; }

    var total    = paras.Count(p => p.IsAttended != null);
    var attended = paras.Count(p => p.IsAttended == true);
    var percent  = service.CalculatePercentage(paras);

    Console.WriteLine("=== Статистика посещаемости ===");
    Console.WriteLine($"  Всего в расписании : {paras.Count}");
    Console.WriteLine($"  Отмечено           : {total}");
    Console.WriteLine($"  Посещено           : {attended}");
    Console.WriteLine($"  Посещаемость       : {percent:F1}%");
    Console.WriteLine();
}
