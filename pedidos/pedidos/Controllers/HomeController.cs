using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pedidos.Data;
using pedidos.Models;

namespace pedidos.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Obtener estadísticas para el dashboard
            var totalPedidos = _context.Orders.Count();
            var totalProductos = _context.Products.Count();
            var totalUsuarios = _context.Users.Count();
            var pedidosPendientes = _context.Orders.Count(o => o.Estado == EstadoPedido.Pendiente);

            var viewModel = new DashboardViewModel
            {
                TotalPedidos = totalPedidos,
                TotalProductos = totalProductos,
                TotalUsuarios = totalUsuarios,
                PedidosPendientes = pedidosPendientes
            };

            return View(viewModel);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardViewModel
    {
        public int TotalPedidos { get; set; }
        public int TotalProductos { get; set; }
        public int TotalUsuarios { get; set; }
        public int PedidosPendientes { get; set; }
    }
}