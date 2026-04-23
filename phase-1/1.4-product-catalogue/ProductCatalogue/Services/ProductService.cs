using ProductCatalogue.Models;
using ProductCatalogue.Repositories;

namespace ProductCatalogue.Services;

public class ProductService(IProductRepository repository)
{
    // Primary constructor (C# 12) — injects the repository
    // Identical concept to Flutter's dependency injection via constructor

    public async Task<IEnumerable<Product>> GetAllAsync() =>
        await repository.GetAllAsync();

    // LINQ: filter + sort — like Dart's .where().toList()
    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        var all = await repository.GetAllAsync();
        return all
            .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Price);
    }

    // LINQ: aggregation — like Python's sum()
    public async Task<decimal> GetTotalValueAsync()
    {
        var all = await repository.GetAllAsync();
        return all.Sum(p => p.Price);
    }

    public async Task<Product?> GetByIdAsync(int id) =>
        await repository.GetByIdAsync(id);

    public async Task<Product> CreateAsync(string name, decimal price, string category)
    {
        // Validate at the service boundary
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        return await repository.CreateAsync(name, price, category);
    }

    public async Task<bool> UpdatePriceAsync(int id, decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));
        return await repository.UpdatePriceAsync(id, newPrice);
    }

    public async Task DeleteAsync(int id)
    {
        var deleted = await repository.DeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"Product with ID {id} not found.");
    }
}