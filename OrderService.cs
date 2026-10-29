using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.EntityFrameworkCore;
using SprintOneStore.Data;
using SprintOneStore.Models;

namespace SprintOneStore.Services;

public sealed record CheckoutLine(Guid ProductId, int Quantity);

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(Guid userId, IReadOnlyCollection<CheckoutLine> cart, string? promoCode = null,
        CancellationToken cancellationToken = default);
}

public sealed class OrderService : IOrderService
{
    private readonly StoreDbContext _db;
    private readonly IDiscountService _discounts;

    public OrderService(StoreDbContext db, IDiscountService discounts)
    {
        _db = db;
        _discounts = discounts;
    }

    public async Task<Order> PlaceOrderAsync(Guid userId, IReadOnlyCollection<CheckoutLine> cart, string? promoCode = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cart);
        if (userId == Guid.Empty) throw new ValidationException("A valid customer is required to place an order.");
        if (cart.Count == 0) throw new ValidationException("Cart is empty.");
        if (cart.Any(line => line.ProductId == Guid.Empty || line.Quantity < 1))
            throw new ValidationException("Every cart line must have a product and a positive quantity.");

        var quantities = cart.GroupBy(line => line.ProductId)
            .ToDictionary(group => group.Key, group => checked(group.Sum(line => line.Quantity)));

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var userExists = await _db.Users.AnyAsync(user => user.Id == userId, cancellationToken);
        if (!userExists) throw new ValidationException("Customer account was not found.");

        var products = await _db.Products.Where(product => quantities.Keys.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);
        if (products.Count != quantities.Count) throw new ValidationException("One or more products no longer exist.");

        decimal subtotal = 0m;
        var orderItems = new List<OrderItem>(quantities.Count);
        foreach (var (productId, quantity) in quantities)
        {
            var product = products[productId];
            if (!product.IsActive) throw new ValidationException($"{product.Name} is not available.");
            if (quantity > product.StockQuantity) throw new ValidationException($"Insufficient stock for {product.Name}.");
            product.StockQuantity -= quantity;
            subtotal += product.UnitPrice * quantity;
            orderItems.Add(new OrderItem { ProductId = productId, Quantity = quantity, UnitPriceAtPurchase = product.UnitPrice });
        }

        var quote = await _discounts.CalculateAsync(promoCode, subtotal, DateTimeOffset.UtcNow, cancellationToken);
        if (quote.PromoCode is not null) quote.PromoCode.RedeemedCount++;

        var order = new Order
        {
            UserId = userId,
            PromoCodeId = quote.PromoCode?.Id,
            PromoCodeSnapshot = quote.PromoCode?.Code,
            Subtotal = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            DiscountAmount = quote.DiscountAmount,
            Total = Math.Max(0m, Math.Round(subtotal - quote.DiscountAmount, 2, MidpointRounding.AwayFromZero)),
            Items = orderItems,
            Status = OrderStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Orders.Add(order);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ValidationException("Stock or promo availability changed during checkout. Refresh the cart and try again.", exception);
        }

        return order;
    }
}
