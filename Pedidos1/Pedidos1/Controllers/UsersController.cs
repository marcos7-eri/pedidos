using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pedidos1.Data;
using Pedidos1.Models;
using Pedidos1.ViewModels;

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

        // LISTA
        public async Task<IActionResult> Index()
        {
            var list = await _db.Users.AsNoTracking().OrderBy(u => u.Name).ToListAsync();
            return View(list);
        }

        // CREATE
        [HttpGet]
        public IActionResult Create() => View(new UserCreateViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var exists = await _db.Users.AnyAsync(u => u.Email == vm.Email);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Email), "El email ya está registrado.");
                return View(vm);
            }

            var user = new User
            {
                Name = vm.Name.Trim(),
                Email = vm.Email.Trim(),
                Role = vm.Role
            };
            user.PasswordHash = _hasher.HashPassword(user, vm.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["msg"] = "Usuario registrado. Ahora puedes iniciar sesión.";
            return RedirectToAction("Login", "Account");
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return NotFound();
            return View(u);
        }

        // EDIT
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();

            var vm = new UserEditViewModel
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var u = await _db.Users.FindAsync(id);
            if (u == null) return NotFound();

            var emailTaken = await _db.Users.AnyAsync(x => x.Email == vm.Email && x.Id != id);
            if (emailTaken)
            {
                ModelState.AddModelError(nameof(vm.Email), "El email ya está en uso.");
                return View(vm);
            }

            u.Name = vm.Name.Trim();
            u.Email = vm.Email.Trim();
            u.Role = vm.Role;

            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
                u.PasswordHash = _hasher.HashPassword(u, vm.NewPassword);

            await _db.SaveChangesAsync();
            TempData["msg"] = "Usuario actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return NotFound();
            return View(u);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u != null)
            {
                _db.Users.Remove(u);
                await _db.SaveChangesAsync();
                TempData["msg"] = "Usuario eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
