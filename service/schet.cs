using System.Collections.Generic;
using System.Linq;
using posechenie.model;

namespace posechenie.service
{
    public class schet : classobyazan
    {
        public double CalculateAttendancePercentage(List<para> paru)
        {
            var attended = paru.Count(l => l.IsAttended == true);
            var total = paru.Count(l => l.IsAttended != null);

            if (total == 0) return 0;

            return (double)attended / total * 100;
        }
    }
}