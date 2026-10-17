using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class CachingCatalogStore(ICatalogStore innerStore) : ICatalogStore
{
    private readonly ICatalogStore _innerStore = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    private List<Product>? _cache;

    public IReadOnlyList<Product> List()
    {
        _cache ??= _innerStore.List().ToList();
        return _cache;
    }

    public Product? GetById(string id)
    {
        // Шукаємо спочатку в кеші, якщо нема — запитуємо у внутрішнього сховища
        var product = _cache?.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (product != null) return product;

        return _innerStore.GetById(id);
    }

    public void Add(Product item)
    {
        _innerStore.Add(item);
        _cache?.Add(item);
    }

    public void Update(Product item)
    {
        _innerStore.Update(item);
        if (_cache != null)
        {
            var index = _cache.FindIndex(p => p.Id.Equals(item.Id, StringComparison.OrdinalIgnoreCase));
            if (index >= 0) _cache[index] = item;
            else _cache.Add(item);
        }
    }

    public bool Remove(string id)
    {
        var success = _innerStore.Remove(id);
        if (success && _cache != null)
        {
            _cache.RemoveAll(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
        return success;
    }
}