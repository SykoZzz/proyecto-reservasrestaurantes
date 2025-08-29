using Microsoft.EntityFrameworkCore;
using TrabajoFinal.Models;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Restaurante> Restaurantes { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
}
