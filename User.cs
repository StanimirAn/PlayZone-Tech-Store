using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SprintOneStore.Models;

public sealed class User : IdentityUser<Guid>
{
    [StringLength(80)]
    public string? DisplayName { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
