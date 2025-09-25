using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pedidos1.Data;
using Pedidos1.Models;
using Pedidos1.ViewModels;
using System.Text.RegularExpressions;

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

        // GET: /Users
        public async Task<IActionResult> Index()
        {
            var list = await _db.Users.AsNoTracking()
                                      .OrderBy(u => u.Name)
                                      .ToListAsync();
            return View(list);
        }

        // GET: /Users/Create
        public IActionResult Create() => View(new UserCreateViewModel());

        // POST: /Users/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel vm)
        {
            // Validación de modelo (Name, Email, Password, Confirm, Role)
            if (!ModelState.IsValid)
                return View(vm);

            // Reglas de contraseña (ajústalas si quieres)
            if (!IsStrongPassword(vm.Password, out var pwdError))
            {
                ModelState.AddModelError(nameof(vm.Password), pwdError);
                return View(vm);
            }

            // Email único
            var exists = await _db.Users.AnyAsync(u => u.Email == vm.Email);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Email), "El email ya está registrado.");
                return View(vm);
            }

            // Mapear a entidad User y hashear
            var user = new User
            {
                Name = vm.Name.Trim(),
                Email = vm.Email.Trim(),
                Role = vm.Role
            };
            user.PasswordHash = _hasher.HashPassword(user, vm.Password);

            _db.Add(user);
            await _db.SaveChangesAsync();

            TempData["msg"] = "Usuario registrado. Ahora puedes iniciar sesión.";
            return RedirectToAction("Login", "Account");
        }

        // Reglas simples de contraseña (mínimo 6, letra y número). Ajusta si quieres.
        private static bool IsStrongPassword(string pwd, out string error)
        {
            error = "";
            if (pwd.Length < 6)
            {
                error = "La contraseña debe tener al menos 6 caracteres.";
                return false;
            }
            if (!Regex.IsMatch(pwd, "[A-Za-z]"))
            {
                error = "La contraseña debe incluir al menos una letra.";
                return false;
            }
            if (!Regex.IsMatch(pwd, "[0-9]"))
            {
                error = "La contraseña debe incluir al menos un número.";
                return false;
            }
            return true;
        }
    }
}
