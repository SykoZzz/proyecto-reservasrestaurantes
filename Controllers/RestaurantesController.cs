namespace appReservas.Controllers;

using Microsoft.AspNetCore.Mvc;
using  appReservas.Data;
using appReservas.Models;

public class RestaurantesController : Controller
{
    private readonly ApplicationDbContext _context;

    public RestaurantesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var restaurantes = _context.Restaurantes.ToList();
        return View(restaurantes);
    }

    public IActionResult Details(int id)
    {
        var restaurante = _context.Restaurantes.FirstOrDefault(r => r.Id == id);
        if (restaurante == null) return NotFound();
        return View(restaurante);
    }
}
