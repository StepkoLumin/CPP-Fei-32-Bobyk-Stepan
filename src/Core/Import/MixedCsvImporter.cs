using Core.Dto;
using System.Text;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<EntityDto> Load(string path)
    {
        var items = new List<EntityDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            if (number == 1 && line.Contains("type", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;

                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<EntityDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string cleanLine = line.Replace("\"", "");

        string[] parts = cleanLine.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["P", var id, var sku, var name, var unit, var qty]
                when int.TryParse(qty, out int q) && q >= 0
                => new ParseOk(
                    new ProductDto(id, sku, name, unit, q)),

            ["W", var id, var name, var address]
                => new ParseOk(
                    new WarehouseDto(id, name, address)),

            ["P", ..]
                => new ParseFailed(
                    "неправильний формат товару або від'ємна кількість"),

            ["W", ..]
                => new ParseFailed(
                    "неправильний формат складу (очікується 4 колонки)"),

            [var type, ..]
                => new ParseFailed(
                    $"невідомий префікс '{type}'"),

            _
                => new ParseFailed(
                    "порожній рядок або невідомий формат")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(EntityDto Value) : ParseOutcome;

    private sealed record ParseFailed(string Reason) : ParseOutcome;
}