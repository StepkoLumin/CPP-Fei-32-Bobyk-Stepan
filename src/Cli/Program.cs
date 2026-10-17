using System;
using System.IO;
using Core;
using Core.Abstractions;
using Core.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Використовуємо фабрику для створення сховища (опційно увімкнувши кешуючий декоратор)
ICatalogStore store = StoreFactory.Create(args, useCaching: true);

var service = new CatalogService(store);

Console.WriteLine($"=== СХОВИЩЕ: {store.GetType().Name} ===");

try
{
    var created = service.Add("SKU-999", "Професійний перфоратор", "шт", 15);
    service.Receive(created.Id, 5);

    Console.WriteLine("\nПоточний список товарів:");
    foreach (var p in service.All())
    {
        Console.WriteLine($"{p.Id} | {p.Sku,-10} | {p.Name,-25} | Кількість: {p.Quantity,3} {p.Unit}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Помилка: {ex.GetType().Name} - {ex.Message}");
}