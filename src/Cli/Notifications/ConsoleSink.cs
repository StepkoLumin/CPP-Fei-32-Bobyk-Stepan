using System;
using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

// Реалізація сповіщень, яка виводить повідомлення в консоль.
public sealed class ConsoleSink : INotificationSink
{
    // Отримує інформацію про зміну товару.
    public void Notify(CatalogChangedEventArgs change)
    {
        // Перевіряємо, чи передано дані події.
        ArgumentNullException.ThrowIfNull(change);

        // Формуємо повідомлення про зміну товару.
        var line = $"{change.At:HH:mm:ss} {change.Kind,-8} " +
                   $"{change.Id} {change.Delta,4:+0;-0} " +
                   $"→ залишок {change.QuantityAfter}";

        // Виводимо повідомлення в консоль.
        Console.WriteLine(line);
    }
}