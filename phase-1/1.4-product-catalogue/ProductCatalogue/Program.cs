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