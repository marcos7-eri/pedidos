using Microsoft.AspNetCore.Mvc;

namespace pedidos.Migrations
{
    public class _20231015000000_InitialCreate : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
