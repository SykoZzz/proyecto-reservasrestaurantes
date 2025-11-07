using Microsoft.ML.Data;
using  appReservas.Models;
namespace appReservas.ML
{
    public class ReservaMLData
    {
        // Características de entrada
        [LoadColumn(0)]
        public string UserId { get; set; }

        [LoadColumn(1)]
        public string Distrito { get; set; } // opcional, si quieres usar ubicación

        [LoadColumn(2)]
        public float Rating { get; set; } // rating del restaurante (feature)

        // Label (la categoría que queremos predecir)
        [LoadColumn(3)]
        public string Tipo { get; set; } 
    }
}
