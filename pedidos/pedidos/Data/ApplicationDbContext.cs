using System;
using Microsoft.EntityFrameworkCore;
using pedidos.Models;

namespace pedidos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Email unico
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            //Estado como string legible
            modelBuilder.Entity<Order>()
                .Property(o => o.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            //Precisiones decimales
            modelBuilder.Entity<Product>()
                .Property(p => p.Precio).HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.Total).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Subtotal).HasPrecision(18, 2);

            //relaciones
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders!)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.OrderItems!)
                .HasForeignKey(i => i.ProductId);
        }
    }
}
