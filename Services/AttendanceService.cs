using raspisanie.Models;

namespace raspisanie.Services;

public class AttendanceService : IAttendanceService // прост считает процент посещённых парё
{
    public double CalculatePercentage(List<Para> paras)
    {
        var total    = paras.Count(p => p.IsAttended != null);
        var attended = paras.Count(p => p.IsAttended == true);

        if (total == 0) return 0;

        return (double)attended / total * 100;
    }
}
