using Core.Abstractions;
using Core.Storage;

namespace Core;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args, bool useCaching = false)
    {
        bool useFile = args.Contains("--file");
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

        ICatalogStore store = useFile
            ? new FileCatalogStore(dataPath)
            : new InMemoryCatalogStore(SampleData.Products());

        // Якщо потрібно — огортаємо у декоратор кешування
        return useCaching ? new CachingCatalogStore(store) : store;
    }
}