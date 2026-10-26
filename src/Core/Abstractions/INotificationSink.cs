using Core.Events;

namespace Core.Abstractions;

// Контракт для компонентів, які отримують сповіщення
// про зміни в каталозі товарів.
public interface INotificationSink
{
    // Приймає інформацію про зміну товару
    // та визначає, як її обробити.
    void Notify(CatalogChangedEventArgs change);
}
