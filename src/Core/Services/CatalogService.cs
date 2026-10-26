using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

// Бізнес-сервіс працює зі сховищем і компонентами сповіщень через інтерфейси.
public sealed class CatalogService(
    ICatalogStore store,
    IEnumerable<INotificationSink>? notificationSinks = null)
{
    // Зберігаємо сховище для роботи з товарами.
    private readonly ICatalogStore _store =
        store ?? throw new ArgumentNullException(nameof(store));

    // Зберігаємо компоненти сповіщень, передані через конструктор.
    // Якщо їх не передали, використовуємо порожній список.
    private readonly IReadOnlyList<INotificationSink> _notificationSinks =
        (notificationSinks ?? Array.Empty<INotificationSink>()).ToArray();

    // Подія повідомляє підписників про зміну товару.
    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // Формує повідомлення після успішної зміни товару.
    private void OnChanged(
        Product product,
        CatalogChangeKind kind,
        int delta)
    {
        // Створюємо об'єкт із даними про зміну.
        var change = new CatalogChangedEventArgs(
            product.Id,
            kind,
            delta,
            product.Quantity);

        // Сповіщаємо підписників події Changed.
        Changed?.Invoke(this, change);

        // Передаємо повідомлення кожному компоненту сповіщень.
        foreach (var sink in _notificationSinks)
        {
            sink.Notify(change);
        }
    }

    // Додаємо новий товар до каталогу.
    public Product Add(
        string sku,
        string name,
        string unit,
        int quantity)
    {
        // Створюємо товар з унікальним ідентифікатором.
        var product = Product.Create(
            Guid.NewGuid().ToString("N")[..8],
            sku,
            name,
            unit,
            quantity);

        // Зберігаємо товар у сховищі.
        _store.Add(product);

        // Повідомляємо про успішне додавання товару.
        OnChanged(product, CatalogChangeKind.Added, quantity);

        return product;
    }

    // Реєструємо надходження товару на склад.
    public void Receive(string id, int quantity)
    {
        // Шукаємо товар за ідентифікатором.
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException(
                $"Немає запису з id={id}.");

        // Змінюємо кількість товару.
        // Якщо перевірка не пройде, подія не викликатиметься.
        product.RegisterArrival(quantity);

        // Зберігаємо оновлений товар.
        _store.Update(product);

        // Повідомляємо про успішне надходження.
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    // Повертаємо всі товари з каталогу.
    public IReadOnlyList<Product> All() => _store.List();

    // Шукаємо товар за ідентифікатором.
    public Product? Find(string id) => _store.GetById(id);

    // Шукаємо товари, які відповідають переданій умові.
    public IReadOnlyList<Product> FindWhere(Func<Product, bool> predicate)
    {
        // Перевіряємо, чи передано умову пошуку.
        ArgumentNullException.ThrowIfNull(predicate);

        return _store.Find(predicate);
    }
}