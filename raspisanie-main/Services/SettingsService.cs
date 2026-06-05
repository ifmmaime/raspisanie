using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using raspisanie.Models;

namespace raspisanie.Services;

// ═══════════════════════════════════════════════════════
// ПАТТЕРН: ОДИНОЧКА (Singleton) — GoF, порождающий
// ═══════════════════════════════════════════════════════
// Единственный экземпляр сервиса на всё приложение.
// Управляет асинхронной записью/чтением JSON-настроек
// с debounce через CancellationTokenSource:
//   каждое новое изменение отменяет предыдущий таймер
//   и запускает новый отсчёт 600 мс.
// ═══════════════════════════════════════════════════════
public class SettingsService
{
    // Singleton: единственный экземпляр, создаётся при первом обращении
    private static readonly SettingsService _instance = new();
    public static SettingsService Instance => _instance;

    private readonly string _filePath = "appsettings.json";

    // Debounce: ссылка на текущий токен отмены
    private CancellationTokenSource? _cts;

    // Флаг «идёт сохранение» + событие для ViewModel
    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        private set
        {
            if (_isSaving == value) return;
            _isSaving = value;
            IsSavingChanged?.Invoke();  // уведомляем подписчиков
        }
    }

    /// <summary>Вызывается при изменении IsSaving — ViewModel подписывается сюда.</summary>
    public event Action? IsSavingChanged;

    // Закрытый конструктор — часть Singleton
    private SettingsService() { }

    // ── Load ──────────────────────────────────────────────
    /// <summary>Читает настройки из JSON синхронно для простоты инициализации при старте.</summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new AppSettings();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    // ── Async Load ────────────────────────────────────────
    /// <summary>Читает настройки из JSON. Если файл не найден — возвращает дефолт.</summary>
    public async Task<AppSettings> LoadAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new AppSettings();

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    // ── Debounced Save ────────────────────────────────────
    /// <summary>
    /// Планирует сохранение с debounce 600 мс.
    /// Каждый вызов отменяет предыдущий таймер — файл записывается
    /// только когда пользователь перестал менять настройки.
    /// </summary>
    public void ScheduleSave(AppSettings settings)
    {
        // Отменяем предыдущий таймер (cancellation)
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // Запускаем debounce в фоне
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(600, token);   // ждём 600 мс

                IsSaving = true;
                await SaveInternalAsync(settings, token);
            }
            catch (OperationCanceledException)
            {
                // Нормально: новое изменение отменило этот таймер
            }
            finally
            {
                IsSaving = false;
            }
        }, token);
    }

    // ── Internal Save ─────────────────────────────────────
    private async Task SaveInternalAsync(AppSettings settings, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json, ct);
    }
}
