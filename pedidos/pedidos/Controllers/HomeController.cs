using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pedidos.Data;
using pedidos.Models;

namespace pedidos.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}