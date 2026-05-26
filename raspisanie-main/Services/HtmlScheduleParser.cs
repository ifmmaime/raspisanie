using HtmlAgilityPack;
using raspisanie.Models;

namespace raspisanie.Services;

public class HtmlScheduleParser // Парсит все расписание в бд по хтмл
{
    public List<Para> Parse(string htmlFilePath)
    {
        var html = File.ReadAllText(htmlFilePath, System.Text.Encoding.UTF8);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var result = new List<Para>();

        var days = doc.DocumentNode.SelectNodes("//div[@class='day']");
        if (days == null) return result;

        foreach (var day in days)
        {
            var dateText = day.SelectSingleNode(".//div[@class='date']/p[1]")?.InnerText.Trim(); // Дата из первого <p> внутри div.date
            if (!DateTime.TryParseExact(dateText, "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var date))
                continue;

            var lessons = day.SelectNodes(".//div[@class='day_lesson']");
            if (lessons == null) continue;

            foreach (var lesson in lessons)
            {
                var time = lesson.SelectSingleNode(".//div[@class='num_time']/p[2]") // Время — второй <p> в div.num_time
                                 ?.InnerText.Trim() ?? "";

                var infos = lesson.SelectNodes(".//div[@class='lessen_info']");
                if (infos == null || infos.Count < 2) continue;

                var subject = infos[0].SelectSingleNode(".//p[@class='name']")?.InnerText.Trim() ?? ""; // первый lessen_info: предмет и преподаватель
                var teacher = infos[0].SelectSingleNode(".//p[not(@class)]")?.InnerText.Trim() ?? "";

                var roomRaw = infos[1].SelectSingleNode(".//p[last()]")?.InnerText.Trim() ?? ""; // Второй lessen_info: аудитория
                var room    = ExtractRoom(roomRaw);

                result.Add(new Para
                {
                    Date    = date,
                    Time    = time,
                    Subject = HtmlEntity.DeEntitize(subject),
                    Teacher = HtmlEntity.DeEntitize(teacher),
                    Room    = room
                });
            }
        }

        return result;
    }

    private static string ExtractRoom(string raw) // Из "Аудитория: 404" берём "404"  (не знаю зачем это нужно но просто чтобы было красиво)
    {
        var m = System.Text.RegularExpressions.Regex.Match(raw, @"[Аа]удитория[:\s]+(\S+)");
        return m.Success ? m.Groups[1].Value : raw;
    }
}
