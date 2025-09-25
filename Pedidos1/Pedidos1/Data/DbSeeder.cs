using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pedidos1.Models;

namespace Pedidos1.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, IPasswordHasher<User> hasher)
        {
            await db.Database.MigrateAsync();

            if (!await db.Users.AnyAsync())
            {
                var admin = new User
                {
                    Name = "Administrador",
                    Email = "admin@demo.com",
                    Role = "admin",
                    PasswordHash = "" // se setea con hasher
                };
                admin.PasswordHash = hasher.HashPassword(admin, "Admin123");
                await db.Users.AddAsync(admin);

                var cliente = new User
                {
                    Name = "Cliente Demo",
                    Email = "cliente@demo.com",
                    Role = "cliente",
                    PasswordHash = ""
                };
                cliente.PasswordHash = hasher.HashPassword(cliente, "Cliente123");
                await db.Users.AddAsync(cliente);

                await db.Products.AddRangeAsync(
                    new Product { Name = "Lapicero", Description = "Azul", Price = 3.50m, Stock = 100, Category = "Utiles" },
                    new Product { Name = "Cuaderno", Description = "100 hojas", Price = 12.00m, Stock = 50, Category = "Utiles" }
                );

                await db.SaveChangesAsync();
            }
        }
    }
}
