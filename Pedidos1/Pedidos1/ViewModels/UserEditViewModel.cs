using System.ComponentModel.DataAnnotations;

namespace Pedidos1.ViewModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = default!;

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = default!;

        [Required, StringLength(20)]
        public string Role { get; set; } = "cliente";

        // Opcional: cambiar contraseña
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string? NewPassword { get; set; }
    }
}
