using System;
using System.IO;
using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

// Реалізація сповіщень, яка записує повідомлення у файл.
public sealed class FileSink : INotificationSink
{
    // Зберігаємо шлях до файлу журналу.
    private readonly string _logPath;

    public FileSink()
    {
        // Формуємо шлях до logs/app.log відносно каталогу програми.
        _logPath = Path.Combine(
            AppContext.BaseDirectory,
            "logs",
            "app.log");

        // Створюємо папку logs, якщо її ще немає.
        Directory.CreateDirectory(
            Path.GetDirectoryName(_logPath)!);
    }

    // Отримує інформацію про зміну товару та записує її у файл.
    public void Notify(CatalogChangedEventArgs change)
    {
        // Перевіряємо, чи передано дані події.
        ArgumentNullException.ThrowIfNull(change);

        // Формуємо рядок журналу.
        var line = $"{change.At:HH:mm:ss} {change.Kind,-8} " +
                   $"{change.Id} {change.Delta,4:+0;-0} " +
                   $"→ залишок {change.QuantityAfter}";

        // Додаємо повідомлення в кінець файлу.
        // Попередні записи при цьому зберігаються.
        File.AppendAllText(
            _logPath,
            line + Environment.NewLine);
    }
}