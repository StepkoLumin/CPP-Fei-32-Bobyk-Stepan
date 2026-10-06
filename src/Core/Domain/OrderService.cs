using System;

namespace Core.Domain;

public class OrderService
{
    public void PlaceOrder(Product product, int requestedAmount)
    {
        if (product.Quantity < requestedAmount)
        {
            throw new InvalidOperationException(
                $"Помилка оформлення: Запитувана кількість ({requestedAmount}) " +
                $"перевищує доступний залишок ({product.Quantity}) для товару {product.Sku}.");
        }

        product.Issue(requestedAmount);
        Console.WriteLine($"[Сервіс] Успішно зарезервовано {requestedAmount} шт. товару {product.Sku}");
    }
}