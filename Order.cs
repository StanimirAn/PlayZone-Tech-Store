using System.ComponentModel.DataAnnotations;

namespace SprintOneStore.Models;

public sealed class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? PromoCodeId { get; set; }
    public PromoCode? PromoCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    [StringLength(32)]
    public string? PromoCodeSnapshot { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
