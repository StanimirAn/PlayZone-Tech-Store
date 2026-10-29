using System.ComponentModel.DataAnnotations;

namespace SprintOneStore.Models;

public sealed class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
