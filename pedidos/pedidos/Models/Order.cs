using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pedidos.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public DateTime FechaPedido { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string Notas { get; set; }

        // Relaciones
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }

    public enum EstadoPedido
    {
        Pendiente,
        Procesando,
        Enviado,
        Entregado,
        Cancelado
    }
}
