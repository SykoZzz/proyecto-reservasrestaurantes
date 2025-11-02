using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace appReservas.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100)]
        public string NombreCompleto { get; set; }  
    }
}