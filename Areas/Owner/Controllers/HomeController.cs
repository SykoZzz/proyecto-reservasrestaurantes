using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using appReservas.Data;
using appReservas.Models;

namespace appReservas.Areas.Owner.Controllers;

[Area("Owner")]
[Authorize(Roles = "Owner")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "restaurant_id")?.Value;

        if (!int.TryParse(claim, out var restauranteId))
        {
            ViewBag.Mensaje = "No tienes un restaurante asignado (claim 'restaurant_id'). Pide al admin que te lo asigne.";
            return View(new Tuple<Restaurante?, List<Reserva>>(null, new List<Reserva>()));
        }

        var restaurante = await _db.Set<Restaurante>().AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == restauranteId);

        if (restaurante == null)
        {
            ViewBag.Mensaje = "El restaurante asignado en tu claim no existe.";
            return View(new Tuple<Restaurante?, List<Reserva>>(null, new List<Reserva>()));
        }

        var reservas = await _db.Set<Reserva>().AsNoTracking()
            .Where(x => x.RestauranteId == restauranteId)
            .OrderBy(x => x.Fecha)
            .ToListAsync();

        return View(new Tuple<Restaurante?, List<Reserva>>(restaurante, reservas));
    }
}