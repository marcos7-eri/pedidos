using System.ComponentModel.DataAnnotations;

namespace Pedidos1.ViewModels
{
    public class UserCreateViewModel
    {
        [Required, StringLength(80)]
        public string Name { get; set; } = default!;

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = default!;

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = default!;

        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required, StringLength(20)]
        public string Role { get; set; } = "cliente";
    }
}
