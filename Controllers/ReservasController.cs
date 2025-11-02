using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using appReservas.Data;
using appReservas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace appReservas.Controllers
{
    [Authorize] // 🔒 Solo usuarios autenticados
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservasController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservasController(ApplicationDbContext context, ILogger<ReservasController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // 📋 Mostrar reservas del usuario logueado
        public IActionResult Index()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para ver tus reservas.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservas = _context.Reservas
                .Include(r => r.Restaurante)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Fecha)
                .ToList();

            _logger.LogInformation("Usuario {UserId} tiene {Cantidad} reservas.", userId, reservas.Count);
            return View(reservas);
        }

        // 🆕 Formulario de creación de reserva
        public IActionResult Create(int restauranteId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para hacer una reserva.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var restaurante = _context.Restaurantes.FirstOrDefault(r => r.Id == restauranteId);
            if (restaurante == null)
            {
                _logger.LogWarning("Intento de reserva para restaurante inexistente: {RestauranteId}", restauranteId);
                return NotFound();
            }

            ViewBag.RestauranteNombre = restaurante.Nombre;
            return View(new Reserva { RestauranteId = restauranteId });
        }

        // ✅ Crear reserva (desde Details o formulario)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int restauranteId, DateTime Fecha, int Personas)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para hacer una reserva.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var restaurante = await _context.Restaurantes.FirstOrDefaultAsync(r => r.Id == restauranteId);
            if (restaurante == null)
            {
                _logger.LogWarning("Restaurante no encontrado (ID: {RestauranteId})", restauranteId);
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("UserId nulo o vacío al intentar crear reserva.");
                TempData["ErrorMessage"] = "Error al identificar al usuario.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // 🔍 Verificar si ya existe reserva para el mismo usuario, restaurante y fecha
            bool yaReservado = await _context.Reservas
                .AnyAsync(r => r.UserId == userId 
                               && r.RestauranteId == restauranteId 
                               && r.Fecha.Date == Fecha.Date);

            if (yaReservado)
            {
                TempData["ErrorMessage"] = "❌ Ya tienes una reserva para este restaurante en la fecha seleccionada.";
                return RedirectToAction("Index"); // 🔹 Redirige a Mis Reservas
            }

            // 🔍 Obtener el usuario actual y su nombre completo
            var user = await _userManager.GetUserAsync(User);
            string nombreCliente = user?.GetType().GetProperty("NombreCompleto")?.GetValue(user)?.ToString()
                                   ?? user?.UserName
                                   ?? "Usuario";

            // 🕓 Ajuste correcto de zona horaria (Perú = UTC-5)
            Fecha = DateTime.SpecifyKind(Fecha, DateTimeKind.Local);

            if (Fecha < DateTime.Now)
            {
                TempData["ErrorMessage"] = "No puedes hacer una reserva para una fecha pasada.";
                return RedirectToAction("Index"); // 🔹 Redirige a Mis Reservas
            }

            var reserva = new Reserva
            {
                RestauranteId = restauranteId,
                Fecha = Fecha,
                Personas = Personas,
                UserId = userId,
                NombreCliente = nombreCliente
            };

            try
            {
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Reserva creada correctamente: Restaurante {RestauranteId}, Usuario {UserId}, Fecha {Fecha}", restauranteId, userId, Fecha);

                TempData["SuccessMessage"] = $"✅ ¡Reserva realizada con éxito, {nombreCliente}!";
                return RedirectToAction("Index"); // 🔹 Redirige a Mis Reservas
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al guardar la reserva: {Mensaje}", ex.InnerException?.Message ?? ex.Message);
                TempData["ErrorMessage"] = "❌ Ocurrió un error al guardar tu reserva. Intenta nuevamente.";
                return RedirectToAction("Index"); // 🔹 Redirige a Mis Reservas
            }
        }

        // ❌ Cancelar una reserva
        public IActionResult Cancel(int id)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para cancelar una reserva.";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reserva = _context.Reservas.FirstOrDefault(r => r.Id == id && r.UserId == userId);

            if (reserva == null)
            {
                _logger.LogWarning("Intento de cancelar reserva no encontrada o ajena. ReservaId: {ReservaId}, Usuario: {UserId}", id, userId);
                return NotFound();
            }

            try
            {
                _context.Reservas.Remove(reserva);
                _context.SaveChanges();

                _logger.LogInformation("Reserva {ReservaId} cancelada por usuario {UserId}", id, userId);
                TempData["SuccessMessage"] = "✅ Reserva cancelada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reserva: {Mensaje}", ex.InnerException?.Message ?? ex.Message);
                TempData["ErrorMessage"] = "❌ No se pudo cancelar la reserva. Intenta nuevamente.";
            }

            return RedirectToAction("Index");
        }
    }
}
