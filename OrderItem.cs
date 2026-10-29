using System.ComponentModel.DataAnnotations;

namespace SprintOneStore.Models;

public sealed class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Range(1, 10000)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal UnitPriceAtPurchase { get; set; }

    public decimal LineTotal => Quantity * UnitPriceAtPurchase;
}
