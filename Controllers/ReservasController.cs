namespace TrabajoFinal.Controllers;
 using Microsoft.EntityFrameworkCore;

 using Microsoft.AspNetCore.Mvc;
 using TrabajoFinal.Data;
 using TrabajoFinal.Models;
public class ReservasController : Controller
{
    private readonly AppDbContext _context;

    public ReservasController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Create(int restauranteId)
    {
        ViewBag.Restaurante = _context.Restaurantes.FirstOrDefault(r => r.Id == restauranteId);
        return View();
    }
    public IActionResult Index()
{
    var reservas = _context.Reservas
        .Include(r => r.Restaurante) 
        .ToList();
    return View(reservas);
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Reserva reserva)
    {
        if (ModelState.IsValid)
        {
            _context.Reservas.Add(reserva);
            _context.SaveChanges();
            return RedirectToAction("Index", "Restaurantes");
        }
        return View(reserva);
    }
}
