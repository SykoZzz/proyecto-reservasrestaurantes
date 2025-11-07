using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PROYECTO_RESERVASRESTAURANTES.Integration.galletafortuna;
using PROYECTO_RESERVASRESTAURANTES.Integration.galletafortuna.dto;



namespace PROYECTO_RESERVASRESTAURANTES.Controllers
{
    
    public class GalletaController : Controller
    {
        private readonly ILogger<GalletaController> _logger;
        private readonly GalletaApiIntegration _apiIntegration;

        public GalletaController(ILogger<GalletaController> logger, GalletaApiIntegration apiIntegration)
        {
            _logger = logger;
            _apiIntegration = apiIntegration ?? throw new ArgumentNullException(nameof(apiIntegration));
        }

        [HttpGet("Galleta")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var mensajeRaw = await _apiIntegration.ObtenerMensajeAsync();

                // Extraer el texto del JSON
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(mensajeRaw);
                var texto = jsonDoc.RootElement.GetProperty("text").GetString();

                // Traducciones manuales básicas
                var traducciones = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Don't spend time. Invest it.", "No gastes tiempo. Inviértelo." },
    { "Your future is bright.", "Tu futuro es brillante." },
    { "Be the change that you wish to see in the world.", "Sé el cambio que deseas ver en el mundo." },
    { "Happiness is an inside job.", "La felicidad viene desde adentro." },
    { "A fresh start will put you on your way.", "Un nuevo comienzo te pondrá en camino." },
    { "In the end, it’s not the years in your life that count. It’s the life in your years.", 
      "Al final, no son los años en tu vida los que cuentan, sino la vida en tus años." }
                };

                if (texto != null && traducciones.ContainsKey(texto))
                {
                    texto = traducciones[texto];
                }

                var galleta = new Galleta { Mensaje = texto };
                return View(galleta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la galleta de la fortuna.");
                return View("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
