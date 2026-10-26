using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    // Символ, який розділяє колонки CSV-файлу.
    private const char Separator = ';';

    // Завантажує товари з CSV-файлу.
    // reportSkippedRow — делегат для повідомлень про пропущені рядки.
    public static ImportResult<ProductDto> Load(
        string path,
        Action<string>? reportSkippedRow = null)
    {
        // Список успішно імпортованих товарів.
        var items = new List<ProductDto>();

        // Список помилок, які виникли під час імпорту.
        var errors = new List<string>();

        // Зчитуємо всі рядки файлу.
        string[] lines = File.ReadAllLines(path);

        // Перебираємо рядки CSV-файлу.
        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            // Пропускаємо порожні рядки та коментарі.
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропускаємо заголовок CSV-файлу.
            if (number == 1 &&
                line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Перевіряємо та обробляємо поточний рядок.
            switch (ParseLine(line))
            {
                case ParseOk ok:
                    // Якщо рядок правильний, додаємо товар до результату.
                    items.Add(ok.Value);
                    break;

                case ParseFailed failed:
                    // Формуємо повідомлення про помилковий рядок.
                    string message = $"Рядок {number}: {failed.Reason}";

                    // Зберігаємо помилку в результатах імпорту.
                    errors.Add(message);

                    // Повідомляємо зовнішній код про пропущений рядок.
                    // Якщо обробник не передали, виклик просто пропускається.
                    reportSkippedRow?.Invoke(message);
                    break;
            }
        }

        // Повертаємо успішно імпортовані товари та список помилок.
        return new ImportResult<ProductDto>(items, errors);
    }

    // Перевіряє та перетворює один рядок CSV на товар.
    private static ParseOutcome ParseLine(string line)
    {
        // Розділяємо рядок на колонки та прибираємо зайві пробіли.
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        // Перевіряємо структуру рядка та його значення.
        return parts switch
        {
            // Для товару потрібно рівно п'ять колонок.
            { Length: < 5 } =>
                new ParseFailed(
                    $"Очікується 5 колонок, отримано {parts.Length}"),

            // SKU або назва товару не повинні бути порожніми.
            [_, "", _, _, _] or [_, _, "", _, _] =>
                new ParseFailed(
                    "SKU або назва товару порожні"),

            // Кількість має бути цілим невід'ємним числом.
            [_, _, _, _, var qty]
                when !int.TryParse(qty, out int q) || q < 0 =>
                new ParseFailed(
                    $"Кількість '{qty}' не є невід'ємним цілим числом"),

            // Якщо всі п'ять колонок правильні, створюємо товар.
            [var id, var sku, var name, var unit, var qty] =>
                new ParseOk(
                    new ProductDto(
                        id,
                        sku,
                        name,
                        unit,
                        int.Parse(qty))),

            // Якщо колонок більше п'яти, рядок також некоректний.
            _ =>
                new ParseFailed(
                    $"Забагато колонок: {parts.Length}")
        };
    }

    // Базовий тип результату перевірки рядка.
    private abstract record ParseOutcome;

    // Результат успішного розбору рядка.
    private sealed record ParseOk(ProductDto Value) : ParseOutcome;

    // Результат невдалого розбору рядка з поясненням помилки.
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}