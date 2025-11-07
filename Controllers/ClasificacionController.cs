using Microsoft.AspNetCore.Mvc;
using appReservas.ML;
using appReservas.Data; 
using Microsoft.EntityFrameworkCore;

namespace appReservas.Controllers
{
    public class ClasificacionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ClasificacionMLService _mlService;

        public ClasificacionController(ApplicationDbContext context)
        {
            _context = context;
            _mlService = new ClasificacionMLService();
        }

        public IActionResult Index()
        {
            return View();
        }

        // Entrenar modelo manualmente
        public IActionResult Entrenar()
        {
            var restaurantes = _context.Restaurantes.ToList();
            if (!restaurantes.Any())
                return Content("No hay datos suficientes para entrenar el modelo.");

            _mlService.EntrenarModelo(restaurantes);
            return Content("✅ Modelo entrenado correctamente y guardado.");
        }

        // Probar predicción para un restaurante
        public IActionResult Predecir(int id)
        {
            var restaurante = _context.Restaurantes.FirstOrDefault(r => r.Id == id);
            if (restaurante == null)
                return NotFound();

            try
            {
                var categoria = _mlService.PredecirCategoria(restaurante);
                ViewBag.Nombre = restaurante.Nombre;
                ViewBag.Categoria = categoria;
                return View("Index");
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }
    }
}
