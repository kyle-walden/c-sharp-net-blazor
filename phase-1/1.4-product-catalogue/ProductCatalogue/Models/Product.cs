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