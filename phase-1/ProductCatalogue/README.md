# ProductCatalogue (Phase 1)

Developer summary

Overview
- Small console application demonstrating a simple MVVM-style layering in plain C#.
- Uses an in-memory repository to store `Product` objects and shows how the ViewModel, Service, and Repository interact.

What it does
- Boots a small composition root in `Program.cs`, creates a `ProductCatalogueViewModel`, and exercises commands (create, update, delete, query).
- Prints application state to the console: product list, filtered queries, total value, error messages and loading state.

Architecture (MVVM mapping)
- View: `Program.cs` — in this console demo the program acts as the View: it invokes ViewModel commands and renders ViewModel state to the console.
- ViewModel: `ViewModels/ProductCatalogueViewModel.cs` — the canonical ViewModel: holds UI state (Products, TotalValue, IsLoading, ErrorMessage) and exposes commands (LoadProductsAsync, AddProductAsync, UpdatePriceAsync, DeleteProductAsync).
- Model: `Models/Product.cs` + the repository interfaces (`Repositories/IProductRepository.cs`) represent the domain model and data access (persistence boundary).
- Service: `Services/ProductService.cs` — implements business rules and validation; the ViewModel calls the service rather than talking to the repository directly.
- Composition root: `Program.cs` wires concrete repository -> service -> viewmodel (manual DI) and demonstrates the MVVM flow.

Key files
- `Program.cs` — app entry + manual DI and example UI interactions.
- `Models/Product.cs` — product entity.
- `Repositories/IProductRepository.cs` and `Repositories/InMemoryProductRepository.cs` — data store abstraction + in-memory implementation.
- `Services/ProductService.cs` — business logic and validation.
- `ViewModels/ProductCatalogueViewModel.cs` — state + commands consumed by the view.

How to run (developer)
Make sure you have the .NET SDK that matches the project target (`TargetFramework` = `net10.0`). From the repository root or this folder:

```bash
cd phase-1/ProductCatalogue
dotnet restore
dotnet build
dotnet run
```

What to expect
- The console app will add a few products, print the catalogue, modify an item, and show error-handling behavior.

Dev notes
- This is intentionally simple and dependency-injection is manual to make the wiring explicit for learning.
- No external database or I/O required — safe to run locally.

