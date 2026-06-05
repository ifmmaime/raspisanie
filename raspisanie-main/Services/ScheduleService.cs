using raspisanie.Data;
using raspisanie.Models;

namespace raspisanie.Services;

public class ScheduleService // Сохраняет список пар в бдшность
{
    private readonly AppDbContext _db;

    public ScheduleService(AppDbContext db) => _db = db;

    public async Task SaveAsync(List<Para> paras)
    {
        if (paras.Count == 0)
        {
            Console.WriteLine("Список занятий пуст.");
            return;
        }

        _db.Paras.AddRange(paras);
        await _db.SaveChangesAsync();

        Console.WriteLine($"Сохранено {paras.Count} занятий.");
    }
}
