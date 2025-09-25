using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos1.Data;
using Pedidos1.Models;

namespace Pedidos1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // LISTA
        public async Task<IActionResult> Index(string? searchString, string? categoryFilter, decimal? minPrice, decimal? maxPrice)
        {
            var q = _db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
                q = q.Where(p => EF.Functions.Like(p.Name, $"%{searchString}%") ||
                                 EF.Functions.Like(p.Description!, $"%{searchString}%"));

            if (!string.IsNullOrWhiteSpace(categoryFilter))
                q = q.Where(p => p.Category == categoryFilter);

            if (minPrice.HasValue) q = q.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) q = q.Where(p => p.Price <= maxPrice.Value);

            var items = await q.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            return View(items);
        }

        // CREATE
        public IActionResult Create() => View(new Product());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);
            _db.Add(product);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Producto creado.";
            return RedirectToAction(nameof(Index));
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return View(p);
        }

        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product input)
        {
            if (!ModelState.IsValid) return View(input);
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();

            p.Name = input.Name;
            p.Description = input.Description;
            p.Price = input.Price;
            p.Stock = input.Stock;
            p.Category = input.Category;

            await _db.SaveChangesAsync();
            TempData["msg"] = "Producto actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p != null)
            {
                _db.Products.Remove(p);
                await _db.SaveChangesAsync();
                TempData["msg"] = "Producto eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
