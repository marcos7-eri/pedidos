using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pedidos.Data;
using pedidos.Models;

namespace PedidosMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public AccountController(ApplicationDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // GET
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (u == null || _hasher.VerifyHashedPassword(u, u.PasswordHash, password) == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Credenciales invalidas";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, u.Nombre),
                new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
                new Claim("email", u.Email),
                new Claim("rol", u.Rol),
                new Claim(ClaimTypes.Role, u.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return !string.IsNullOrEmpty(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction("Index", "Home");
        }

        // GET
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET
        public IActionResult AccessDenied() => Content("Acceso denegado.");
    }
}
