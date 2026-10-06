using System;
using System.IO;
using System.Collections.Generic;
using Core.Domain;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("=== Сценарій 1: успіх ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine(product);

product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product); // Має показати залишок 120

Console.WriteLine("\n=== Сценарій 2: порушення інваріантів ===");
TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("порожній SKU", () => Product.Create("P-002", "", "Пісок", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));


Console.WriteLine("\n=== ДОДАТКОВЕ ЗАВДАННЯ 1: Зв'язок Лаби 3 і Лаби 4 ===");
string csvPath = Path.Combine("data", "sample.csv");
if (File.Exists(csvPath))
{
    var dtoResult = ProductCsvImporter.Load(csvPath);
    
    var validDomainProducts = new List<Product>();
    var domainErrors = new List<string>(dtoResult.Errors);

    foreach (var dto in dtoResult.Items)
    {
        try
        {
            Product domainProduct = Product.FromDto(dto);
            validDomainProducts.Add(domainProduct);
        }
        catch (Exception ex)
        {
            domainErrors.Add($"! Помилка домену в записі {dto.Id}: {ex.Message}");
        }
    }

    Console.WriteLine($"Успішно створено доменних об'єктів: {validDomainProducts.Count}");
    Console.WriteLine($"Загальна кількість помилок (парсер + домен): {domainErrors.Count}");
    
    foreach (var e in domainErrors)
    {
        Console.WriteLine(e);
    }
}


Console.WriteLine("\n=== ДОДАТКОВЕ ЗАВДАННЯ 2: Сервіс та 2 сутності ===");
var service = new OrderService();
var myProduct = Product.Create("P-020", "SKU-ORDER", "Спецтовар", "шт", 50);

TryDo("Спроба замовити 100 шт при залишку 50", () => service.PlaceOrder(myProduct, 100));


// --- Допоміжна функція для відлову помилок (має бути в самому кінці) ---
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" {title}: {ex.GetType().Name} - {ex.Message}");
    }
}

Console.WriteLine("\n=== ДОДАТКОВЕ ЗАВДАННЯ 3: Стан-перелічування (Enum) ===");
var myOrder = Order.Create("ORD-777");
Console.WriteLine(myOrder); // Покаже Draft

// 1. Успішний перехід: Draft -> Confirmed
myOrder.ChangeStatus(OrderStatus.Confirmed);
Console.WriteLine($"Після оновлення: {myOrder}");

// 2. Некоректний перехід: Confirmed -> Cancelled (заборонено нашими правилами)
TryDo("Спроба скасувати вже підтверджене замовлення", () => myOrder.ChangeStatus(OrderStatus.Cancelled));