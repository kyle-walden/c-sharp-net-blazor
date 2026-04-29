// Repositories/InMemoryProductRepository.cs
using ProductCatalogue.Models;

namespace ProductCatalogue.Repositories;

public class InMemoryProductRepository : IProductRepository
{
    // Private in-memory "database" — like a List in Dart
    private readonly List<Product> _products = [];
    private int _nextId = 1;

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        // Task.FromResult wraps a value in a completed Task
        // — same as Dart's Future.value(x)
        return Task.FromResult<IEnumerable<Product>>(_products);
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<Product> CreateAsync(string name, decimal price, string category)
    {
        var product = new Product
        {
            Id = _nextId++,
            Name = name,
            Price = price,
            Category = category
        };
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task<bool> UpdatePriceAsync(int id, decimal newPrice)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult(false);

        product.Price = newPrice;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult(false);

        _products.Remove(product);
        return Task.FromResult(true);
    }
}