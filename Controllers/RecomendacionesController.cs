using Microsoft.AspNetCore.Mvc;
using appReservas.ML;
using System.Security.Claims;

namespace appReservas.Controllers
{
    public class RecomendacionesController : Controller
    {
        private readonly RecomendacionMLService _mlService;

        public RecomendacionesController(RecomendacionMLService mlService)
        {
            _mlService = mlService;
        }

        // GET: /Recomendaciones
        public IActionResult Index()
        {
            return View();
        }

        // Acción que devuelve partial view con recomendaciones para el usuario actual
        [HttpGet]
        public IActionResult MostrarParaUsuario(string userId = null)
        {
            // Si userId no viene, intentar obtener del usuario logueado
            if (string.IsNullOrEmpty(userId) && User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            if (string.IsNullOrEmpty(userId))
            {
                return PartialView("_RecomendacionesPartial", new List<Models.Restaurante>());
            }

            // Si no hay modelo entrenado, intenta entrenar (puede lanzar si hay pocos datos)
            try
            {
                // Si el modelo aún no existe, este call entrenará al vuelo
                // Puedes evitar entrenar en cada petición verificando si existe el archivo (se maneja en el servicio)
                var recomendaciones = _mlService.RecomendarRestaurantes(userId, 6);
                return PartialView("~/Views/Restaurantes/_RecomendacionesPartial.cshtml", recomendaciones);
            }
            catch (Exception ex)
            {
                // En producción registra el error; aquí devolvemos fallback
                var fallback = new List<Models.Restaurante>(); 
                return PartialView("_RecomendacionesPartial", fallback);
            }
        }
    }
}
