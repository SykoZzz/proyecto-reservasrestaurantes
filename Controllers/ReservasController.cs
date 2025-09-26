using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using appReservas.Data;
using appReservas.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace appReservas.Controllers
{
    [Authorize] // Solo usuarios autenticados
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(ApplicationDbContext context, ILogger<ReservasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para ver tus reservas.";
                return RedirectToAction("Login", "Account"); // Ajusta al controlador de login que tengas
            }

            _logger.LogInformation("Accediendo a Mis Reservas");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservas = _context.Reservas
                .Include(r => r.Restaurante)
                .Where(r => r.UserId == userId)
                .ToList();

            _logger.LogInformation($"Usuario {userId} tiene {reservas.Count} reservas");
            return View(reservas);
        }

        public IActionResult Create(int restauranteId)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para hacer una reserva.";
                return RedirectToAction("Login", "Account");
            }

            var restaurante = _context.Restaurantes.FirstOrDefault(r => r.Id == restauranteId);
            if (restaurante == null) return NotFound();

            ViewBag.RestauranteNombre = restaurante.Nombre;
            return View(new Reserva { RestauranteId = restauranteId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reserva reserva)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para hacer una reserva.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                reserva.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _context.Reservas.Add(reserva);
                _context.SaveChanges();

                _logger.LogInformation("Reserva creada para usuario {UserId} en restaurante {RestauranteId}", reserva.UserId, reserva.RestauranteId);

                return RedirectToAction("Index");
            }

            _logger.LogWarning("Error al crear la reserva");
            return View(reserva);
        }

        public IActionResult Cancel(int id)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para cancelar una reserva.";
                return RedirectToAction("Login", "Account");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reserva = _context.Reservas.FirstOrDefault(r => r.Id == id && r.UserId == userId);

            if (reserva == null)
            {
                _logger.LogWarning("Reserva {ReservaId} no encontrada para usuario {UserId}", id, userId);
                return NotFound();
            }

            _context.Reservas.Remove(reserva);
            _context.SaveChanges();

            _logger.LogInformation("Reserva {ReservaId} cancelada por usuario {UserId}", id, userId);

            return RedirectToAction("Index");
        }
    }
}
