// Repositories/IProductRepository.cs
using ProductCatalogue.Models;

namespace ProductCatalogue.Repositories;

// Like an abstract class / interface in Dart
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(string name, decimal price, string category);
    Task<bool> UpdatePriceAsync(int id, decimal newPrice);
    Task<bool> DeleteAsync(int id);
}