using FoodOrderAPI.Domain.Entities;
using FoodOrderAPI.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderAPI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId);

                entity.Property(oi => oi.UnitPrice)
                    .HasConversion(m => m.Amount, a => new Money(a))
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<OrderStatusHistory>()
                .HasOne(h => h.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderId);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => new {o.Status, o.CreatedAt})
                    .HasDatabaseName("IX_Orders_Status_CreatedAt");

                entity.HasIndex(o => new {o.Type, o.CreatedAt})
                    .HasDatabaseName("IX_Orders_Type_CreatedAt");

                entity.HasIndex(o   => o.CreatedAt)
                    .HasDatabaseName("IX_Order_CreatedAt");

                entity.Property(o => o.Total)
                    .HasConversion(m => m.Amount, a => new Money(a))
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.IsActive)
                    .HasDatabaseName("IX_Product_IsActive");

                entity.Property(p => p.Price)
                    .HasConversion(m => m.Amount, a => new Money(a))
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_User_Email");
            });
    }
}