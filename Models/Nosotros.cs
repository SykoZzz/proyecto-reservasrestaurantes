using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PROYECTO_RESERVASRESTAURANTES.Models
{
    public class Nosotros
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public List<string> Restaurantes { get; set; }

        // Cuestionario
        [Required]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Correo { get; set; }

        [Required]
        public string Mensaje { get; set; }
    }
}
