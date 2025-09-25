using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos1.Data;
using Pedidos1.Models;

namespace Pedidos1.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController(ApplicationDbContext db, IPasswordHasher<User> hasher) : Controller
    {
        public async Task<IActionResult> Index() => View(await db.Users.AsNoTracking().ToListAsync());

        public IActionResult Create() => View(new User());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            try
            {
                if (!ModelState.IsValid) return View(user);
                if (await db.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError(nameof(User), "El email ya existe.");
                    return View(user);
                }
                user.PasswordHash = hasher.HashPassword(user, password);
                db.Add(user);
                await db.SaveChangesAsync();
                TempData["msg"] = "Usuario creado.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(user);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User input, string? newPassword)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null) return NotFound();

            try
            {
                if (!ModelState.IsValid) return View(input);

                user.Name = input.Name;
                user.Email = input.Email;
                user.Role = input.Role;

                if (!string.IsNullOrWhiteSpace(newPassword))
                    user.PasswordHash = hasher.HashPassword(user, newPassword);

                await db.SaveChangesAsync();
                TempData["msg"] = "Usuario actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(input);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await db.Users.FindAsync(id);
            if (user != null)
            {
                db.Users.Remove(user);
                await db.SaveChangesAsync();
                TempData["msg"] = "Usuario eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
