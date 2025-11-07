using Microsoft.ML.Data;
using  appReservas.Models;
namespace appReservas.ML;
public class ClasificacionInput
{
    public float PrecioPromedio { get; set; }
    public float Rating { get; set; }
    public string Tipo { get; set; }
    public string Distrito { get; set; }
    public string CategoriaPrecio { get; set; } // etiqueta (label)
}