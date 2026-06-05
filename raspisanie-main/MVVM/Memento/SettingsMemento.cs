namespace raspisanie.MVVM.Memento;

// ═══════════════════════════════════════════════════════
// ПАТТЕРН: ХРАНИТЕЛЬ (Memento) — GoF, поведенческий
// Роль: MEMENTO — неизменяемый снимок состояния.
// Хранит копию данных AppSettings в определённый момент.
// Не содержит логики — только данные.
// ═══════════════════════════════════════════════════════
public record SettingsMemento(string HtmlFilePath, string DefaultTab);
