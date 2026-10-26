namespace Core.Events;

// Визначає тип зміни, яка відбулася з товаром на складі.
public enum CatalogChangeKind
{
    Added,    // Додано новий товар
    Received, // Товар надійшов на склад
    Issued,   // Товар видано зі складу
    Removed   // Товар видалено з каталогу
}