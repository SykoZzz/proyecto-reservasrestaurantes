namespace TrabajoFinal.Models;
public class Restaurante
{
    public int Id { get; set; }
    public string ?Nombre { get; set; }
    public string ?Tipo { get; set; }
    public string Direccion { get; set; }
    public string Distrito { get; set; }
    public decimal PrecioPromedio { get; set; }
    public decimal Rating { get; set; }
    public string ImagenUrl { get; set; }
}
