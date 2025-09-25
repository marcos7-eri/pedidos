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

        // GET: /Products
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

        // GET: /Products/Create
        public IActionResult Create() => View(new Product());

        // POST: /Products/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            _db.Add(product);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Producto creado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
