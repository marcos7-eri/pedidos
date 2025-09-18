using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pedidos.Data;
using pedidos.Models;

namespace PedidosMVC.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isEmpleado = User.IsInRole("Empleado");

            IQueryable<Order> orders;

            if (isAdmin || isEmpleado)
            {
                orders = _context.Orders.Include(o => o.User);
            }
            else
            {
                orders = _context.Orders.Where(o => o.UserId == int.Parse(userId));
            }

            return View(await orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Verificar que el usuario tenga acceso a este pedido
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && !User.IsInRole("Empleado") && order.UserId != int.Parse(userId))
            {
                return Forbid();
            }

            return View(order);
        }

        // GET: Orders/Create
        public async Task<IActionResult> Create()
        {
            // Cargar productos para seleccionar
            ViewBag.Products = await _context.Products.Where(p => p.Activo && p.Stock > 0).ToListAsync();
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderItems,Notas")] OrderCreateViewModel viewModel)
        {
            if (ModelState.IsValid && viewModel.OrderItems.Any(oi => oi.Cantidad > 0))
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var order = new Order
                    {
                        UserId = userId,
                        FechaPedido = DateTime.Now,
                        Estado = EstadoPedido.Pendiente,
                        Notas = viewModel.Notas
                    };

                    // Calcular total y validar stock
                    decimal total = 0;
                    foreach (var item in viewModel.OrderItems.Where(oi => oi.Cantidad > 0))
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product == null || !product.Activo)
                        {
                            ModelState.AddModelError("", $"El producto con ID {item.ProductId} no existe.");
                            await transaction.RollbackAsync();
                            ViewBag.Products = await _context.Products.Where(p => p.Activo && p.Stock > 0).ToListAsync();
                            return View(viewModel);
                        }

                        if (product.Stock < item.Cantidad)
                        {
                            ModelState.AddModelError("", $"Stock insuficiente para {product.Nombre}. Stock disponible: {product.Stock}");
                            await transaction.RollbackAsync();
                            ViewBag.Products = await _context.Products.Where(p => p.Activo && p.Stock > 0).ToListAsync();
                            return View(viewModel);
                        }

                        var subtotal = product.Precio * item.Cantidad;
                        total += subtotal;

                        order.OrderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = product.Precio,
                            Subtotal = subtotal
                        });

                        // Reducir stock
                        product.Stock -= item.Cantidad;
                        _context.Products.Update(product);
                    }

                    order.Total = total;
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Error al crear el pedido: " + ex.Message);
                }
            }
            else if (!viewModel.OrderItems.Any(oi => oi.Cantidad > 0))
            {
                ModelState.AddModelError("", "Debe seleccionar al menos un producto con cantidad mayor a 0.");
            }

            ViewBag.Products = await _context.Products.Where(p => p.Activo && p.Stock > 0).ToListAsync();
            return View(viewModel);
        }

        // POST: Orders/UpdateStatus/5
        [HttpPost]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, EstadoPedido nuevoEstado)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Estado = nuevoEstado;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }

    public class OrderCreateViewModel
    {
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public string Notas { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public int Cantidad { get; set; }
    }
}