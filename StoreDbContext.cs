using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SprintOneStore.Models;

namespace SprintOneStore.Data;

public sealed class StoreDbContext : IdentityDbContext<User, Role, Guid>
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>().HasIndex(product => product.Sku).IsUnique();
        builder.Entity<Category>().HasIndex(category => category.Name).IsUnique();
        builder.Entity<PromoCode>().HasIndex(code => code.Code).IsUnique();
        builder.Entity<Product>().Property(product => product.UnitPrice).HasPrecision(10, 2);
        builder.Entity<Order>().Property(order => order.Subtotal).HasPrecision(10, 2);
        builder.Entity<Order>().Property(order => order.DiscountAmount).HasPrecision(10, 2);
        builder.Entity<Order>().Property(order => order.Total).HasPrecision(10, 2);
        builder.Entity<OrderItem>().Property(item => item.UnitPriceAtPurchase).HasPrecision(10, 2);
        builder.Entity<PromoCode>().Property(code => code.Value).HasPrecision(10, 2);
        builder.Entity<PromoCode>().Property(code => code.MinimumOrderAmount).HasPrecision(10, 2);
        builder.Entity<Product>().Property(product => product.RowVersion).IsRowVersion();
        builder.Entity<PromoCode>().Property(code => code.RowVersion).IsRowVersion();

        builder.Entity<Product>().HasOne(product => product.Category).WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Order>().HasOne(order => order.User).WithMany(user => user.Orders)
            .HasForeignKey(order => order.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Order>().HasOne(order => order.PromoCode).WithMany(code => code.Orders)
            .HasForeignKey(order => order.PromoCodeId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<OrderItem>().HasOne(item => item.Order).WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<OrderItem>().HasOne(item => item.Product).WithMany(product => product.OrderItems)
            .HasForeignKey(item => item.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Review>().HasOne(review => review.User).WithMany(user => user.Reviews)
            .HasForeignKey(review => review.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Review>().HasOne(review => review.Product).WithMany(product => product.Reviews)
            .HasForeignKey(review => review.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
