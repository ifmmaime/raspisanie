using raspisanie.MVVM.Memento;

namespace raspisanie.Models;

// ═══════════════════════════════════════════════════════
// ПАТТЕРН: ХРАНИТЕЛЬ (Memento) — GoF, поведенческий
// Роль: ORIGINATOR — объект, чьё состояние сохраняется.
// Создаёт снимок (SettingsMemento) и умеет восстанавливаться из него.
// ═══════════════════════════════════════════════════════
public class AppSettings
{
    /// <summary>Путь к HTML-файлу расписания.</summary>
    public string HtmlFilePath { get; set; } = "raspisanie.html";

    /// <summary>Вкладка, открытая по умолчанию.</summary>
    public string DefaultTab { get; set; } = "Сегодня";

    // Originator: создаёт снимок текущего состояния
    public SettingsMemento CreateMemento()
        => new SettingsMemento(HtmlFilePath, DefaultTab);

    // Originator: восстанавливает состояние из снимка
    public void Restore(SettingsMemento memento)
    {
        HtmlFilePath = memento.HtmlFilePath;
        DefaultTab   = memento.DefaultTab;
    }
}
