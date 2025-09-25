using System.ComponentModel.DataAnnotations;

namespace Pedidos1.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = default!;

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = default!;

        [Required] // Se guarda hash, no texto plano
        public string PasswordHash { get; set; } = default!;

        // admin / cliente / empleado
        [Required, StringLength(20)]
        public string Role { get; set; } = "cliente";
    }
}
