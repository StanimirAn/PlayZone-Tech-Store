using System.ComponentModel.DataAnnotations;
using SprintOneStore.Models;

namespace SprintOneStore.Services;

public interface IShoppingCartService
{
    void Add(ShoppingCart cart, Product product, int quantity = 1);
    void SetQuantity(ShoppingCart cart, Guid productId, int quantity);
    void Remove(ShoppingCart cart, Guid productId);
    void Clear(ShoppingCart cart);
}

public sealed class ShoppingCartService : IShoppingCartService
{
    public void Add(ShoppingCart cart, Product product, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(product);
        if (!product.IsActive) throw new ValidationException("This product is not available.");
        if (quantity < 1) throw new ValidationException("Quantity must be at least one.");

        var existing = cart.Items.SingleOrDefault(item => item.ProductId == product.Id);
        var requestedTotal = quantity + (existing?.Quantity ?? 0);
        if (requestedTotal > product.StockQuantity) throw new ValidationException("Requested quantity exceeds available stock.");

        if (existing is null)
        {
            cart.Items.Add(new CartLine { ProductId = product.Id, ProductName = product.Name, Quantity = quantity, UnitPrice = product.UnitPrice });
        }
        else
        {
            existing.Quantity = requestedTotal;
        }
    }

    public void SetQuantity(ShoppingCart cart, Guid productId, int quantity)
    {
        ArgumentNullException.ThrowIfNull(cart);
        if (quantity < 1) throw new ValidationException("Quantity must be at least one. Use Remove to delete a cart line.");
        var line = cart.Items.SingleOrDefault(item => item.ProductId == productId)
            ?? throw new KeyNotFoundException("The product is not in this cart.");
        line.Quantity = quantity;
    }

    public void Remove(ShoppingCart cart, Guid productId)
    {
        ArgumentNullException.ThrowIfNull(cart);
        cart.Items.RemoveAll(item => item.ProductId == productId);
    }

    public void Clear(ShoppingCart cart)
    {
        ArgumentNullException.ThrowIfNull(cart);
        cart.Items.Clear();
    }
}
