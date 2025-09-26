using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace appReservas.Data;
using appReservas.Models;
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }
    public DbSet<Restaurante> Restaurantes { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
}
