using System.Collections.Generic;

namespace raspisanie.MVVM.Memento;

// ═══════════════════════════════════════════════════════
// ПАТТЕРН: ХРАНИТЕЛЬ (Memento) — GoF, поведенческий
// Роль: CARETAKER — управляет историей снимков.
// Хранит стек снимков, позволяет откатиться (undo) к предыдущему
// состоянию, не зная внутренней структуры AppSettings.
// ═══════════════════════════════════════════════════════
public class SettingsCaretaker
{
    private readonly Stack<SettingsMemento> _history = new();

    /// <summary>Сохранить снимок в историю.</summary>
    public void Push(SettingsMemento memento) => _history.Push(memento);

    /// <summary>Извлечь последний снимок (undo). Возвращает null, если история пуста.</summary>
    public SettingsMemento? Pop() => _history.Count > 0 ? _history.Pop() : null;

    /// <summary>Есть ли что отменить.</summary>
    public bool CanUndo => _history.Count > 0;
}
