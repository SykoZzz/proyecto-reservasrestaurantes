using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using appReservas.Data;
using appReservas.Models;

namespace appReservas.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RestaurantsController : Controller
{
    private readonly ApplicationDbContext _db;
    public RestaurantsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.Restaurantes.OrderBy(r => r.Nombre).ToListAsync();
        return View(list);
    }

    [HttpGet] public IActionResult Create() => View(new Restaurante());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Restaurante m)
    {
        if (!ModelState.IsValid) return View(m);
        _db.Restaurantes.Add(m);
        await _db.SaveChangesAsync();
        TempData["ok"] = "Restaurante creado.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var r = await _db.Restaurantes.FindAsync(id);
        if (r is null) return NotFound();
        return View(r);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Restaurante m)
    {
        if (!ModelState.IsValid) return View(m);
        _db.Restaurantes.Update(m);
        await _db.SaveChangesAsync();
        TempData["ok"] = "Restaurante actualizado.";
        return RedirectToAction(nameof(Index));
    }
}
