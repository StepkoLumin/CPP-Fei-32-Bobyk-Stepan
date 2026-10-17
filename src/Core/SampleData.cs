using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IReadOnlyList<Product> Products() =>
    [
        Product.Create("p1", "SKU-001", "Цемент М400", "шт", 100),
        Product.Create("p2", "SKU-002", "Пісок будівельний", "т", 20),
        Product.Create("p3", "SKU-003", "Цегла червона", "шт", 1000)
    ];
}