using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using appReservas.Data;
using appReservas.Areas.Admin.Models;
using appReservas.Models;

namespace appReservas.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OwnersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public OwnersController(ApplicationDbContext db, UserManager<IdentityUser> um, RoleManager<IdentityRole> rm)
    { _db = db; _userManager = um; _roleManager = rm; }

    public async Task<IActionResult> Index()
    {
        var owners = await _userManager.GetUsersInRoleAsync("Owner");
        var lista = new List<(IdentityUser user, string? restaurante)>();

        foreach (var u in owners)
        {
            var claim = (await _userManager.GetClaimsAsync(u)).FirstOrDefault(c => c.Type == "restaurant_id")?.Value;
            string? nombre = null;
            if (int.TryParse(claim, out var rid))
                nombre = await _db.Restaurantes.Where(r => r.Id == rid).Select(r => r.Nombre).FirstOrDefaultAsync();

            lista.Add((u, nombre));
        }

        return View(lista);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new CreateOwnerVM
        {
            Restaurantes = await _db.Restaurantes
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = string.IsNullOrWhiteSpace(r.Nombre) ? $"#{r.Id}" : r.Nombre })
                .ToListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOwnerVM vm)
    {
        async Task Fill()
        {
            vm.Restaurantes = await _db.Restaurantes
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = string.IsNullOrWhiteSpace(r.Nombre) ? $"#{r.Id}" : r.Nombre })
                .ToListAsync();
        }

        if (!ModelState.IsValid) { await Fill(); return View(vm); }

        if (!await _roleManager.RoleExistsAsync("Owner"))
        { ModelState.AddModelError("", "Rol 'Owner' no existe. Créalo manualmente (OWNER)."); await Fill(); return View(vm); }

        if (await _userManager.FindByEmailAsync(vm.Email) is not null)
        { ModelState.AddModelError("", "Email ya registrado."); await Fill(); return View(vm); }

        var user = new IdentityUser { UserName = vm.Email, Email = vm.Email, EmailConfirmed = true };
        var create = await _userManager.CreateAsync(user, vm.Password);
        if (!create.Succeeded) { foreach (var e in create.Errors) ModelState.AddModelError("", e.Description); await Fill(); return View(vm); }

        var role = await _userManager.AddToRoleAsync(user, "Owner");
        if (!role.Succeeded) { await _userManager.DeleteAsync(user); foreach (var e in role.Errors) ModelState.AddModelError("", e.Description); await Fill(); return View(vm); }

        if (vm.RestauranteId.HasValue)
        {
            if (!await _db.Restaurantes.AnyAsync(r => r.Id == vm.RestauranteId.Value))
            { ModelState.AddModelError("", "Restaurante no existe."); await Fill(); return View(vm); }

            await _userManager.AddClaimAsync(user, new Claim("restaurant_id", vm.RestauranteId.Value.ToString()));
        }

        TempData["ok"] = "Dueño creado y (si aplicaba) asignado a restaurante.";
        return RedirectToAction(nameof(Index));
    }
}
