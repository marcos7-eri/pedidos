using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pedidos1.Data;
using Pedidos1.Models;

namespace Pedidos1.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _hasher;

        public UsersController(ApplicationDbContext db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // LISTADO DE USUARIOS
        public async Task<IActionResult> Index()
        {
            var list = await _db.Users.AsNoTracking().OrderBy(u => u.Name).ToListAsync();
            return View(list);
        }

        // GET: /Users/Create
        public IActionResult Create() => View(new User { Role = "cliente" });

        // POST: /Users/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("", "La contraseña es obligatoria.");

            if (!ModelState.IsValid) return View(user);

            var exists = await _db.Users.AnyAsync(u => u.Email == user.Email);
            if (exists)
            {
                ModelState.AddModelError(nameof(User), "El email ya está registrado.");
                return View(user);
            }

            user.PasswordHash = _hasher.HashPassword(user, password);
            _db.Add(user);
            await _db.SaveChangesAsync();

            TempData["msg"] = "Usuario creado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
