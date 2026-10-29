namespace SprintOneStore.Models;

public sealed class CartLine
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal => Quantity * UnitPrice;
}

public sealed class ShoppingCart
{
    public List<CartLine> Items { get; } = [];
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public int ItemCount => Items.Sum(item => item.Quantity);
}
