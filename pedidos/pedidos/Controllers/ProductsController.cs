using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pedidos.Data;
using pedidos.Models;

namespace PedidosMVC.Controllers
{
    [Authorize(Roles = "Admin,Empleado")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, string categoria, decimal? precioMin, decimal? precioMax)
        {
            var products = from p in _context.Products
                           where p.Activo
                           select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Nombre.Contains(searchString) || p.Descripcion.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                products = products.Where(p => p.Categoria == categoria);
            }

            if (precioMin.HasValue)
            {
                products = products.Where(p => p.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                products = products.Where(p => p.Precio <= precioMax.Value);
            }

            // Obtener categorías para el dropdown
            ViewBag.Categorias = await _context.Products
                .Where(p => p.Categoria != null)
                .Select(p => p.Categoria)
                .Distinct()
                .ToListAsync();

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Precio,Stock,Categoria")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.Activo = true;
                product.FechaCreacion = DateTime.Now;

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.Activo)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio,Stock,Categoria,Activo")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            // Eliminación lógica
            product.Activo = false;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id && e.Activo);
        }
    }
}