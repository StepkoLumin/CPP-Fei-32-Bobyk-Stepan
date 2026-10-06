using System;

namespace Core.Domain;

// Наш стан-перелічування
public enum OrderStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public sealed class Order
{
    public string Id { get; }
    public OrderStatus Status { get; private set; } // Змінювати ззовні не можна

    private Order(string id)
    {
        Id = id;
        Status = OrderStatus.Draft; // Усі нові замовлення починають як чорновик
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID не може бути порожнім", nameof(id));
            
        return new Order(id);
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        // Перевірка допустимих переходів через сучасний switch expression
        bool isValidTransition = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => true,  // Можна підтвердити
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,  // Можна скасувати
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => false, // Вже підтверджене скасувати не можна
            (OrderStatus.Cancelled, OrderStatus.Draft) => false, // Зі скасованого в чорновик не можна
            _ => false // Будь-які інші (наприклад, Draft -> Draft) заборонені
        };

        if (!isValidTransition)
            throw new InvalidOperationException(
                $"Заборонений перехід: неможливо перевести замовлення зі статусу {Status} у {newStatus}.");

        Status = newStatus;
    }

    public override string ToString() => $"Замовлення {Id} [Статус: {Status}]";
}