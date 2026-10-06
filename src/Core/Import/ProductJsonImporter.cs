using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        // Налаштування: ігнорувати регістр властивостей (id == Id)
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        string json = File.ReadAllText(path, System.Text.Encoding.UTF8);
        
        // Використовуємо оператор ?? [] з методички
        var items = JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? [];
        
        // Оскільки JSON або парситься весь, або падає, список помилок поки порожній
        return new ImportResult<ProductDto>(items, new List<string>());
    }
}