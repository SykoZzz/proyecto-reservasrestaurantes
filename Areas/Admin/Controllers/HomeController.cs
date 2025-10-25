using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using appReservas.Data;

namespace appReservas.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    { _db = db; _userManager = userManager; }

    public async Task<IActionResult> Index()
    {
        ViewBag.Restaurantes = await _db.Restaurantes.CountAsync();
        ViewBag.Owners = (await _userManager.GetUsersInRoleAsync("Owner")).Count;
        return View();
    }
}
