using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SprintOneStore.Data;
using SprintOneStore.Models;

namespace SprintOneStore.Services;

public sealed record DiscountQuote(PromoCode? PromoCode, decimal DiscountAmount);

public interface IDiscountService
{
    Task<DiscountQuote> CalculateAsync(string? code, decimal subtotal, DateTimeOffset now, CancellationToken cancellationToken = default);
}

public sealed class DiscountService : IDiscountService
{
    private readonly StoreDbContext _db;

    public DiscountService(StoreDbContext db) => _db = db;

    public async Task<DiscountQuote> CalculateAsync(string? code, decimal subtotal, DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        if (subtotal < 0) throw new ValidationException("Subtotal cannot be negative.");
        if (string.IsNullOrWhiteSpace(code)) return new DiscountQuote(null, 0m);

        var normalizedCode = code.Trim().ToUpperInvariant();
        var promo = await _db.PromoCodes.SingleOrDefaultAsync(item => item.Code == normalizedCode, cancellationToken)
            ?? throw new ValidationException("Promo code is not valid.");

        if (!promo.IsActive || (promo.StartsAt.HasValue && promo.StartsAt.Value > now)
            || (promo.ExpiresAt.HasValue && promo.ExpiresAt.Value <= now))
            throw new ValidationException("Promo code is inactive or outside its validity period.");
        if (promo.MaxRedemptions.HasValue && promo.RedeemedCount >= promo.MaxRedemptions.Value)
            throw new ValidationException("Promo code has reached its redemption limit.");
        if (promo.MinimumOrderAmount.HasValue && subtotal < promo.MinimumOrderAmount.Value)
            throw new ValidationException("Order does not meet the promo code minimum.");
        if (promo.Value <= 0 || (promo.DiscountType == DiscountType.Percentage && promo.Value > 100))
            throw new ValidationException("Promo code has an invalid discount value.");

        var rawDiscount = promo.DiscountType switch
        {
            DiscountType.Percentage => subtotal * promo.Value / 100m,
            DiscountType.FixedAmount => promo.Value,
            _ => throw new ValidationException("Promo code has an unsupported discount type.")
        };
        var discount = Math.Min(subtotal, Math.Round(rawDiscount, 2, MidpointRounding.AwayFromZero));
        return new DiscountQuote(promo, discount);
    }
}
