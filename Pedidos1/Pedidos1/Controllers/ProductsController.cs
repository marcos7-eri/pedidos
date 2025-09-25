using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos1.Data;
using Pedidos1.Models;

namespace Pedidos1.Controllers
{
    [Authorize(Roles = "admin,empleado")]
    public class ProductsController(ApplicationDbContext db) : Controller
    {
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchString, string? categoryFilter, decimal? minPrice, decimal? maxPrice)
        {
            var q = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                q = q.Where(p => EF.Functions.Like(p.Name, $"%{searchString}%") ||
                                 EF.Functions.Like(p.Description!, $"%{searchString}%"));

            if (!string.IsNullOrWhiteSpace(categoryFilter))
                q = q.Where(p => p.Category == categoryFilter);

            if (minPrice.HasValue)
                q = q.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                q = q.Where(p => p.Price <= maxPrice.Value);

            var items = await q.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            return View(items);
        }

        public IActionResult Create() => View(new Product());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);
            db.Add(product);
            await db.SaveChangesAsync();
            TempData["msg"] = "Producto creado.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product input)
        {
            if (!ModelState.IsValid) return View(input);
            var product = await db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = input.Name;
            product.Description = input.Description;
            product.Price = input.Price;
            product.Stock = input.Stock;
            product.Category = input.Category;

            await db.SaveChangesAsync();
            TempData["msg"] = "Producto actualizado.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var p = await db.Products.FindAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var p = await db.Products.FindAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var p = await db.Products.FindAsync(id);
            if (p != null)
            {
                db.Products.Remove(p);
                await db.SaveChangesAsync();
                TempData["msg"] = "Producto eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
