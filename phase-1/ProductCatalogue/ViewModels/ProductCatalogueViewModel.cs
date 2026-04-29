// ViewModels/ProductCatalogueViewModel.cs
// state management layer
using ProductCatalogue.Models;
using ProductCatalogue.Services;

namespace ProductCatalogue.ViewModels;

// Flutter analogy: this is your ChangeNotifier / Riverpod AsyncNotifier
// Blazor analogy: this is the @code { } block inside a .razor component
public class ProductCatalogueViewModel(ProductService service)
{
    // ---------------------------------------------------------------
    // STATE — like public fields in a Flutter ChangeNotifier
    // The View reads these to decide what to render
    // ---------------------------------------------------------------
    public IEnumerable<Product> Products { get; private set; } = [];
    public decimal TotalValue { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public bool IsLoading { get; private set; }

    // ---------------------------------------------------------------
    // COMMANDS — like methods called from Flutter's onPressed / onTap
    // The View calls these; the ViewModel updates its own state
    // ---------------------------------------------------------------

    // Load / refresh — like Flutter's ref.read(provider.notifier).loadProducts()
    public async Task LoadProductsAsync()
    {
        IsLoading = true;
        Products = await service.GetAllAsync();
        TotalValue = await service.GetTotalValueAsync();
        ErrorMessage = string.Empty;
        IsLoading = false;
    }

    public async Task AddProductAsync(string name, decimal price, string category)
    {
        try
        {
            await service.CreateAsync(name, price, category);
            await LoadProductsAsync();  // refresh state after mutation
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;  // expose error to View — no throw
        }
    }

    public async Task UpdatePriceAsync(int id, decimal newPrice)
    {
        try
        {
            await service.UpdatePriceAsync(id, newPrice);
            await LoadProductsAsync();
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            await service.DeleteAsync(id);
            await LoadProductsAsync();
        }
        catch (KeyNotFoundException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    // Query helper — returns filtered data without mutating state
    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category) =>
        await service.GetByCategoryAsync(category);
}