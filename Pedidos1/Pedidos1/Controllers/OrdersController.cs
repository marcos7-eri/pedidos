using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos1.Data;
using Pedidos1.Models;
using System.Security.Claims;

namespace Pedidos1.Controllers
{
    [Authorize]
    public class OrdersController(ApplicationDbContext db) : Controller
    {
        // Listado y detalle
        public async Task<IActionResult> Index()
        {
            var isAdminOrEmp = User.IsInRole("admin") || User.IsInRole("empleado");
            var q = db.Orders.Include(o => o.Customer).Include(o => o.Items).ThenInclude(i => i.Product).AsQueryable();

            if (!isAdminOrEmp)
            {
                var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                q = q.Where(o => o.CustomerId == uid);
            }

            var list = await q.AsNoTracking().OrderByDescending(o => o.CreatedAt).ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await db.Orders.Include(o => o.Customer)
                                       .Include(o => o.Items).ThenInclude(i => i.Product)
                                       .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            // Seguridad: solo dueño o staff
            var isOwner = order.CustomerId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!(isOwner || User.IsInRole("admin") || User.IsInRole("empleado")))
                return Forbid();

            return View(order);
        }

        // Creación rápida: un solo producto + cantidad
        [Authorize(Roles = "admin,empleado,cliente")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "admin,empleado,cliente")]
        public async Task<IActionResult> Create(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                ModelState.AddModelError("", "La cantidad debe ser mayor que cero.");
                return await Create();
            }

            var product = await db.Products.FindAsync(productId);
            if (product == null) { ModelState.AddModelError("", "Producto inválido."); return await Create(); }

            if (product.Stock < quantity)
            {
                ModelState.AddModelError("", "Stock insuficiente.");
                return await Create();
            }

            var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = new Order
            {
                CustomerId = uid,
                Status = OrderStatus.Pendiente
            };

            var item = new OrderItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                Subtotal = product.Price * quantity
            };

            order.Items.Add(item);
            order.Total = order.Items.Sum(i => i.Subtotal);

            // Ajuste de stock
            product.Stock -= quantity;

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            TempData["msg"] = "Pedido creado.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        // Cambiar estado
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "admin,empleado")]
        public async Task<IActionResult> ChangeStatus(int id, OrderStatus status)
        {
            var order = await db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            await db.SaveChangesAsync();
            TempData["msg"] = "Estado actualizado.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Eliminar pedido (solo admin)
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var o = await db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return NotFound();
            return View(o);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var o = await db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
            if (o != null)
            {
                // Reponer stock al borrar
                foreach (var it in o.Items)
                {
                    var prod = await db.Products.FindAsync(it.ProductId);
                    if (prod != null) prod.Stock += it.Quantity;
                }
                db.Orders.Remove(o);
                await db.SaveChangesAsync();
                TempData["msg"] = "Pedido eliminado y stock repuesto.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
