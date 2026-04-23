# Phase 1.4 — In-Memory Product Catalogue (CRUD + MVVM)

> Goal: Build a C# console app that manages products in memory using classes, LINQ, async methods, and the **MVVM architecture pattern**.  
> Analogies from your Flutter/Flask background are included throughout.

---

## What You're Building

A CLI product catalogue with full CRUD using the MVVM architecture pattern — the same pattern Flutter's official architecture guide recommends.

| MVVM Layer | Responsibility | File(s) |
|---|---|---|
| **Model** | Domain data shape | `Models/Product.cs` |
| **Repository** | In-memory data store (source of truth) | `Repositories/IProductRepository.cs`, `InMemoryProductRepository.cs` |
| **Service** | Business logic — validation, LINQ queries | `Services/ProductService.cs` |
| **ViewModel** | UI state + commands; zero UI knowledge | `ViewModels/ProductCatalogueViewModel.cs` |
| **View** | Render ViewModel state; call ViewModel commands | `Program.cs` |

> **Flutter analogy**: `Product` = your model class, `InMemoryProductRepository` = a local data source, `ProductService` = a repository in Flutter MVVM terms, `ProductCatalogueViewModel` = your `ChangeNotifier`/Riverpod `Notifier`, `Program.cs` = your widget `build()` method.

No database. No HTTP. Just pure C# fundamentals in a console app.

---

## [x] Step 1 — Scaffold the Project 

```bash
cd phase-1/1.4-product-catalogue
dotnet new console -n ProductCatalogue
cd ProductCatalogue
```

Open the folder in VS Code:
```bash
code .
```

You'll see:
```
ProductCatalogue/
├── ProductCatalogue.csproj   # like pubspec.yaml
├── Program.cs                # entry point — like main() in Dart
```

By the end you'll have this structure:
```
ProductCatalogue/
├── Models/
│   └── Product.cs                        # MODEL layer
├── Repositories/
│   ├── IProductRepository.cs             # REPOSITORY layer (interface)
│   └── InMemoryProductRepository.cs      # REPOSITORY layer (implementation)
├── Services/
│   └── ProductService.cs                 # SERVICE layer (business logic)
├── ViewModels/
│   └── ProductCatalogueViewModel.cs      # VIEWMODEL layer (state + commands)
├── Program.cs                            # VIEW layer (thin — renders state, calls commands)
└── ProductCatalogue.csproj
```

---

## [x] Step 2 — Define the `Product` Model

Create a new file: `Models/Product.cs`

```bash
mkdir Models
```

```csharp
// Models/Product.cs
namespace ProductCatalogue.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;

    // Like Dart's toString() override
    public override string ToString() =>
        $"[{Id}] {Name} — ${Price:F2} ({Category})";
}
```

**Why `string.Empty`?**  
C# requires non-nullable properties to be initialised. `string.Empty` is the idiomatic default (equivalent to `""`) — same idea as Dart's `late` or a default value.

---

## [x] Step 3 — Define the Repository Interface

Create `Repositories/IProductRepository.cs`

```bash
mkdir Repositories
```

```csharp
// Repositories/IProductRepository.cs
// this is the contract for our data layer — it defines what operations we can perform on products, but not how they're implemented. The rest of the app will depend on this interface, not the concrete implementation, which allows us to swap out the in-memory version for a real database later without changing any other code.
// CRUD operations + async signatures — same as a Flutter repository interface
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
```

**Why an interface?**  
This is the DI pattern you'll use everywhere in ASP.NET. The interface defines the contract; the concrete class does the work. Think of it like a Dart abstract class.

---

## [x] Step 4 — Implement the In-Memory Repository

Create `Repositories/InMemoryProductRepository.cs`

```csharp
// Repositories/InMemoryProductRepository.cs
// this is our fake "database" — it implements the repository interface but just uses a List<Product> in memory. In Phase 2 this will be replaced by a real database implementation, but the rest of the app won't need to change at all because it depends on the interface, not the implementation.
// this is like FirestoreService extends IProductRepository in Flutter — it implements the same methods but with a different data source.
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
```

> **Note on `Task.FromResult`**: Because there's no real I/O here, we wrap the results in already-completed Tasks. In Phase 2 this will be replaced by real `await db.Products...` calls with EF Core.

---

## [x] Step 5 — Define the Service Layer

Create `Services/ProductService.cs`

```bash
mkdir Services
```

This layer sits between the ViewModel and the repository (step 3). It holds business logic (validation, LINQ filtering). In Flutter MVVM terms this is the **Repository** layer — it's the last thing before raw data storage.

```csharp
// Services/ProductService.cs
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
```

---

## [x] Step 6 — Create the ViewModel

This is the new layer. Create `ViewModels/ProductCatalogueViewModel.cs`

```bash
mkdir ViewModels
```

The ViewModel is the bridge between the data layers (Service/Repository) and the View (Program.cs). It:
- **Holds state** — the current product list, total value, error messages (like fields in a Flutter `ChangeNotifier`)
- **Exposes commands** — async methods the View calls in response to user actions (like `onPressed` handlers)
- **Never knows about the UI** — no `Console.WriteLine`, no framework references

```csharp
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
```

**Why catch instead of throw?**  
In a UI context (Flutter or Blazor), you don't want unhandled exceptions crashing the app — you set `ErrorMessage` so the View can display it gracefully. This is the same pattern as Flutter's `AsyncError` state in `AsyncValue`.

---

## [x] Step 7 — Wire Up `Program.cs` (the View)

The View's only job is to call ViewModel commands and render ViewModel state. It should contain **no business logic** — just like a Flutter widget's `build()` method.

Replace the contents of `Program.cs`:

```csharp
// Program.cs — the VIEW in MVVM
// Rule: only call vm commands and print vm state. No logic lives here.
using ProductCatalogue.Repositories;
using ProductCatalogue.Services;
using ProductCatalogue.ViewModels;

// Composition root — manually wire the dependency graph
// In ASP.NET (Phase 2) this becomes builder.Services.AddScoped<...>()
var repo = new InMemoryProductRepository();
var service = new ProductService(repo);
var vm = new ProductCatalogueViewModel(service);  // ← the ViewModel

Console.WriteLine("=== Product Catalogue (MVVM) ===\n");

// --- COMMAND: Add products ---
Console.WriteLine(">> Adding products (calling ViewModel commands)...");
await vm.AddProductAsync("Keyboard", 79.99m, "Electronics");
await vm.AddProductAsync("Notebook", 4.49m, "Stationery");
await vm.AddProductAsync("Monitor", 299.00m, "Electronics");
await vm.AddProductAsync("Pen", 1.99m, "Stationery");

// --- RENDER STATE: vm.Products ---
Console.WriteLine("\n>> All products (vm.Products):");
foreach (var p in vm.Products)
    Console.WriteLine($"  {p}");

// --- RENDER STATE: vm.TotalValue ---
Console.WriteLine($"\n>> Total catalogue value (vm.TotalValue): ${vm.TotalValue:F2}");

// --- QUERY: filter by category (does not mutate vm state) ---
Console.WriteLine("\n>> Electronics only (sorted by price):");
foreach (var p in await vm.GetByCategoryAsync("Electronics"))
    Console.WriteLine($"  {p}");

// --- COMMAND: Update price ---
Console.WriteLine("\n>> Updating Keyboard price to $59.99 (ViewModel command)...");
await vm.UpdatePriceAsync(1, 59.99m);
Console.WriteLine($"  Updated: {vm.Products.First(p => p.Id == 1)}");

// --- COMMAND: Delete ---
Console.WriteLine("\n>> Deleting Pen (ViewModel command)...");
await vm.DeleteProductAsync(4);

Console.WriteLine("\n>> Products after delete (vm.Products):");
foreach (var p in vm.Products)
    Console.WriteLine($"  {p}");

// --- RENDER STATE: vm.ErrorMessage ---
// View shows error state — ViewModel caught the exception and set ErrorMessage
Console.WriteLine("\n>> Trying to delete non-existent product (ID 99)...");
await vm.DeleteProductAsync(99);
Console.WriteLine($"  vm.ErrorMessage: \"{vm.ErrorMessage}\"");

// --- RENDER STATE: vm.IsLoading ---
// (In Blazor this drives a <p>Loading...</p> conditional — same concept)
Console.WriteLine($"\n>> vm.IsLoading after operations: {vm.IsLoading}");
```

---

## Step 8 — Run It

```bash
dotnet run
```

Expected output:
```
=== Product Catalogue (MVVM) ===

>> Adding products (calling ViewModel commands)...

>> All products (vm.Products):
  [1] Keyboard — $79.99 (Electronics)
  [2] Notebook — $4.49 (Stationery)
  [3] Monitor — $299.00 (Electronics)
  [4] Pen — $1.99 (Stationery)

>> Total catalogue value (vm.TotalValue): $385.47

>> Electronics only (sorted by price):
  [1] Keyboard — $79.99 (Electronics)
  [3] Monitor — $299.00 (Electronics)

>> Updating Keyboard price to $59.99 (ViewModel command)...
  Updated: [1] Keyboard — $59.99 (Electronics)

>> Deleting Pen (ViewModel command)...

>> Products after delete (vm.Products):
  [1] Keyboard — $59.99 (Electronics)
  [2] Notebook — $4.49 (Stationery)
  [3] Monitor — $299.00 (Electronics)

>> Trying to delete non-existent product (ID 99)...
  vm.ErrorMessage: "Product with ID 99 not found."

>> vm.IsLoading after operations: False
```

---

## Step 9 — Stretch Goals (optional, do these if you have time)

These will give you reps on concepts you'll use heavily in Phase 2.

### 9.1 Add `SearchAsync` to the ViewModel
Add the method to `ProductService` first, then expose it as a command on the ViewModel:
```csharp
// ProductService.cs
public async Task<IEnumerable<Product>> SearchAsync(string query)
{
    var all = await repository.GetAllAsync();
    return all.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
}

// ProductCatalogueViewModel.cs — expose as a query helper (no state mutation)
public async Task<IEnumerable<Product>> SearchAsync(string query) =>
    await service.SearchAsync(query);
```

### 9.2 Add a `SearchResults` state property
Instead of returning data from a method, store it on the ViewModel (pure MVVM — View reads state, doesn't hold it):
```csharp
// In ProductCatalogueViewModel
public IEnumerable<Product> SearchResults { get; private set; } = [];

public async Task SearchAsync(string query)
{
    SearchResults = await service.SearchAsync(query);
}
```
This mirrors how Riverpod's `AsyncNotifier` works — the View just reads `vm.SearchResults`.

### 9.3 Make it interactive
Replace the hardcoded `Program.cs` with a menu loop (the View becomes more realistic):
```csharp
// Program.cs — interactive View
while (true)
{
    Console.WriteLine("\n1) List all  2) Add  3) Update price  4) Delete  5) Exit");
    var choice = Console.ReadLine();
    switch (choice)
    {
        case "1":
            foreach (var p in vm.Products) Console.WriteLine($"  {p}");
            break;
        case "2":
            Console.Write("Name: "); var name = Console.ReadLine() ?? "";
            Console.Write("Price: "); var price = decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Category: "); var cat = Console.ReadLine() ?? "";
            await vm.AddProductAsync(name, price, cat);
            if (!string.IsNullOrEmpty(vm.ErrorMessage))
                Console.WriteLine($"  Error: {vm.ErrorMessage}");
            break;
        // ... add cases for 3, 4, 5
    }
}
```

### 9.4 Group with LINQ in the ViewModel
Add a query method that returns grouped data — the View just iterates it:
```csharp
// ProductCatalogueViewModel.cs
public async Task<IEnumerable<(string Category, int Count, decimal AvgPrice)>> GetSummaryAsync()
{
    var all = await service.GetAllAsync();
    return all
        .GroupBy(p => p.Category)
        .Select(g => (g.Key, g.Count(), g.Average(p => p.Price)));
}
```

---

## Concepts Practised in This Task

| C# Concept | Where Used |
|---|---|
| Classes + properties | `Product.cs` |
| Interface | `IProductRepository` |
| Primary constructor (C# 12) | `ProductService(IProductRepository repo)`, `ProductCatalogueViewModel(ProductService service)` |
| `async`/`await` + `Task<T>` | All service/repo/viewmodel methods |
| `Task.FromResult` | Wrapping sync values as Tasks in the repository |
| LINQ: `.Where`, `.OrderBy`, `.Sum`, `.GroupBy` | `ProductService` |
| `string?` nullable + null check | `GetByIdAsync` return type |
| `throw` on invalid input | Service validation |
| `override ToString()` | `Product` model |
| **MVVM: ViewModel state** | `Products`, `TotalValue`, `ErrorMessage`, `IsLoading` on the ViewModel |
| **MVVM: ViewModel commands** | `AddProductAsync`, `UpdatePriceAsync`, `DeleteProductAsync` on the ViewModel |
| **MVVM: thin View** | `Program.cs` — only calls commands, only reads state |

---

## What's Next

Once this is working, you're ready for **Phase 2**. Here's how the MVVM layers you built here map forward:

| This app | Phase 2 (ASP.NET Web API) | Phase 3 (Blazor Server) |
|---|---|---|
| `InMemoryProductRepository` | `EfProductRepository` using EF Core + SQLite | Same repository, different UI |
| `ProductService` | Same class, registered via `builder.Services.AddScoped<>()` | Same |
| `ProductCatalogueViewModel` | Controller (routing) + Service (logic) splits this role | `@code { }` block *is* the ViewModel |
| `Program.cs` (View) | HTTP client / Swagger UI / Postman | `.razor` HTML markup |

The **repository interface stays exactly the same** — you just write a new implementation class and swap it at the composition root:
```csharp
// Phase 2: Program.cs (ASP.NET)
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
//                                               ↑ new implementation, same interface
```
This is the power of programming to interfaces — your Service and ViewModel never need to change when the data source changes.
