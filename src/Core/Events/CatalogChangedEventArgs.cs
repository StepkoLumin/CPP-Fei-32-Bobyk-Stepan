namespace Core.Events;

// Зберігає інформацію про зміну, яка відбулася з товаром.
public sealed class CatalogChangedEventArgs(
    string id,
    CatalogChangeKind kind,
    int delta,
    int quantityAfter) : EventArgs
{
    // Ідентифікатор товару, з яким відбулася зміна.
    public string Id { get; } = id;

    // Тип зміни: додавання, надходження, видача або видалення.
    public CatalogChangeKind Kind { get; } = kind;

    // Кількість одиниць, на яку змінився запас товару.
    public int Delta { get; } = delta;

    // Залишок товару після виконання операції.
    public int QuantityAfter { get; } = quantityAfter;

    // Час, коли було сформовано повідомлення про зміну.
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}