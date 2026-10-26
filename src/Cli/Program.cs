using System;
using System.IO;
using Core;
using Core.Abstractions;
using Core.Events;
using Core.Services;
using Core.Import;
using Cli.Notifications;

// Налаштовуємо відображення українських символів у консолі.
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Створюємо сховище та основний сервіс.
ICatalogStore store = StoreFactory.Create(args, useCaching: true);
var service = new CatalogService(store);

Console.WriteLine("=== ЛАБОРАТОРНА РОБОТА №6 ===");
Console.WriteLine($"Сховище: {store.GetType().Name}");

// Визначаємо шлях до файлу журналу подій.
var logPath = Path.Combine(
    AppContext.BaseDirectory,
    "logs",
    "app.log");

// Створюємо папку для журналу, якщо її ще немає.
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// Обробник події: виводить повідомлення в консоль і записує у файл.
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    // Формуємо повідомлення про зміну товару.
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} " +
               $"{e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";

    // Виводимо повідомлення в консоль.
    Console.WriteLine(line);

    // Записуємо повідомлення у файл журналу.
    try
    {
        File.AppendAllText(logPath, line + Environment.NewLine);
    }
    catch (IOException ex)
    {
        Console.WriteLine($"Помилка запису журналу: {ex.Message}");
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"Немає доступу до журналу: {ex.Message}");
    }
}

// Лічильник кількості отриманих подій.
int changes = 0;

// Перший підписник — логування в консоль і файл.
service.Changed += OnCatalogChanged;

// Другий підписник — лямбда, яка рахує кількість подій.
service.Changed += (_, _) => changes++;

try
{
    // 1. Демонструємо подію додавання товару.
    Console.WriteLine("\n--- 1. Додавання товару ---");

    var created = service.Add(
        "SKU-999",
        "Професійний перфоратор",
        "шт",
        15);

    // 2. Демонструємо подію надходження товару.
    Console.WriteLine("\n--- 2. Надходження товару ---");

    service.Receive(created.Id, 5);

    Console.WriteLine($"Подій після успішних операцій: {changes}");

    // 3. Перевіряємо помилкову операцію.
    Console.WriteLine("\n--- 3. Перевірка помилки ---");

    try
    {
        // Товару з таким ID не існує, тому подія не виникне.
        service.Receive("fake-id-123", 10);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Помилку перехоплено: {ex.Message}");
    }

    // Лічильник має залишитися незмінним після помилки.
    Console.WriteLine($"Подій після помилки: {changes}");

    // 4. Перевіряємо відписку від події.
    Console.WriteLine("\n--- 4. Перевірка відписки ---");

    // Відписуємо логер, але залишаємо лямбду-лічильник.
    service.Changed -= OnCatalogChanged;

    // Кількість товару зміниться без повідомлення від логера.
    service.Receive(created.Id, 5);

    Console.WriteLine($"Подій після тихої операції: {changes}");

    // Перевіряємо, що зміна товару справді відбулася.
    var updated = service.Find(created.Id);
    Console.WriteLine($"Поточна кількість товару: {updated?.Quantity}");

    // 5. Демонструємо використання Func для пошуку.
    Console.WriteLine("\n--- 5. Пошук через Func ---");

    // Знаходимо товари, кількість яких менша за 10.
    var lowStock = service.FindWhere(p => p.Quantity < 10);

    foreach (var product in lowStock)
    {
        Console.WriteLine(
            $"Знайдено: {product.Name}, кількість: {product.Quantity}");
    }

    if (lowStock.Count == 0)
    {
        Console.WriteLine("Товарів із кількістю менше 10 немає.");
    }

    // 6. Додаткове завдання №1: INotificationSink.
    Console.WriteLine("\n=== ДОДАТКОВЕ ЗАВДАННЯ №1: NOTIFICATION SINKS ===");

    // Передаємо сервісу два компоненти через конструктор:
    // ConsoleSink виводить повідомлення, FileSink записує їх у файл.
    var sinkService = new CatalogService(
        store,
        new INotificationSink[]
        {
            new ConsoleSink(),
            new FileSink()
        });

    // Окремий підписник події — для порівняння механізмів.
    sinkService.Changed += (_, e) =>
    {
        Console.WriteLine(
            $"[EVENT] Подія {e.Kind}, товар {e.Id}");
    };

    // Додаємо тестовий товар.
    var testProduct = sinkService.Add(
        "SKU-DI-01",
        "Тест DI",
        "шт",
        2);

    // Реєструємо надходження — перевіряємо другий тип події.
    sinkService.Receive(testProduct.Id, 1);

    // 7. Додаткове завдання №2: Action<string>.
    Console.WriteLine("\n=== ДОДАТКОВЕ ЗАВДАННЯ №2: ACTION<string> ===");

    // Шлях до CSV-файлу з даними товарів відносно поточної папки.
    var csvPath = Path.Combine("data", "sample.csv");

    if (File.Exists(csvPath))
    {
        // Передаємо Action<string>, який повідомляє про пропущені рядки.
        var result = ProductCsvImporter.Load(
            csvPath,
            message => Console.WriteLine($"[ПРОПУЩЕНО] {message}"));

        // Показуємо підсумки імпорту.
        Console.WriteLine(
            $"Успішно імпортовано товарів: {result.Items.Count}");

        Console.WriteLine(
            $"Кількість помилок: {result.Errors.Count}");
    }
    else
    {
        // Повідомляємо, якщо CSV-файл не знайдено.
        Console.WriteLine(
            $"CSV-файл не знайдено: {Path.GetFullPath(csvPath)}");

        Console.WriteLine(
            "Для перевірки Action<string> потрібен файл data/sample.csv.");
    }

    // Виводимо шлях до журналу.
    Console.WriteLine("\nДемонстрацію завершено.");
    Console.WriteLine($"Журнал подій: {logPath}");
}
catch (Exception ex)
{
    // Перехоплюємо неочікувані помилки під час виконання.
    Console.WriteLine($"Помилка програми: {ex.Message}");
}