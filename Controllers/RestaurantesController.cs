using Microsoft.AspNetCore.Mvc;
using appReservas.Data;
using appReservas.Models;
using Microsoft.Extensions.Configuration; // Para leer la API Key
using System.Linq;

namespace appReservas.Controllers
{
    public class RestaurantesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RestaurantesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Vista que muestra todos los restaurantes (sin API Key)
        public IActionResult Index()
        {
            var restaurantes = _context.Restaurantes.ToList();
            return View(restaurantes);
        }

        // Vista de detalles de un restaurante específico (con API Key para Google Maps)
        public IActionResult Details(int id)
        {
            var restaurante = _context.Restaurantes.FirstOrDefault(r => r.Id == id);
            if (restaurante == null) return NotFound();

            // Pasamos la API Key solo a Details
            ViewData["GoogleMapsApiKey"] = _configuration["GoogleMaps:ApiKey"];

            return View(restaurante);
        }
    }
}
