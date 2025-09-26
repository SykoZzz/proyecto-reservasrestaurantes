using System.ComponentModel.DataAnnotations;
namespace appReservas.Models;

public class Reserva
{
    public int Id { get; set; }

    [Required]
    public int RestauranteId { get; set; }

    [Required, StringLength(100)]
    public string NombreCliente { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Required, Range(1, 20)]
    public int Personas { get; set; }

    public Restaurante Restaurante { get; set; }
    
     public string UserId { get; set; } 

}
