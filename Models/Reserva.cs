using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 👈 importante para usar [Column]

namespace appReservas.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un restaurante.")]
        public int RestauranteId { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre del Cliente")]
        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "Debe ingresar una fecha y hora para la reserva.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha y Hora")]
        [Column(TypeName = "timestamp without time zone")] // ✅ evita problemas de zona horaria
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "Debe indicar el número de personas.")]
        [Range(1, 20, ErrorMessage = "El número de personas debe estar entre 1 y 20.")]
        [Display(Name = "Número de Personas")]
        public int Personas { get; set; }

        // 🔹 Relación con restaurante
        public Restaurante Restaurante { get; set; }

        [Required]
        public string UserId { get; set; }

        // 💳 NUEVO: Estado del pago (pendiente / pagado)
        [Display(Name = "Estado del Pago")]
        [StringLength(20)]
        public string EstadoPago { get; set; } = "Pendiente";

        // 💰 NUEVO: Precio de la reserva (puedes cambiar el valor por persona o fijo)
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio Total")]
        public decimal Precio { get; set; } = 20.00m;

        // 🧾 NUEVO: ID de sesión de Stripe (para validar pagos completados)
        [StringLength(255)]
        public string? StripeSessionId { get; set; }
    }
}
