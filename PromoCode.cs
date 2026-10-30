using System.ComponentModel.DataAnnotations;

namespace SprintOneStore.Models;

public sealed class PromoCode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(32, MinimumLength = 4)]
    [RegularExpression("^[A-Z0-9-]+$")]
    public string Code { get; set; } = string.Empty;

    public DiscountType DiscountType { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Value { get; set; }

    public decimal? MinimumOrderAmount { get; set; }
    public DateTimeOffset? StartsAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int? MaxRedemptions { get; set; }
    public int RedeemedCount { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
