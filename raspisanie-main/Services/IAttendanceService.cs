using raspisanie.Models;

namespace raspisanie.Services;


public interface IAttendanceService // просто интерфейс чтобы не ругался код
{
    double CalculatePercentage(List<Para> paras);
}
