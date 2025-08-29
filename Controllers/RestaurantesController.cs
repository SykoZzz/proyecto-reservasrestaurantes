namespace TrabajoFinal.Controllers;

using Microsoft.AspNetCore.Mvc;
using TrabajoFinal.Data;
using TrabajoFinal.Models;

public class RestaurantesController : Controller
{
    private readonly AppDbContext _context;

    public RestaurantesController(AppDbContext context)
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
