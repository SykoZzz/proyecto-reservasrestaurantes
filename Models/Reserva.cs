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

        // Relaciones
        public Restaurante Restaurante { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
