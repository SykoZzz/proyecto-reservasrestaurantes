using Microsoft.AspNetCore.Mvc;
using PROYECTO_RESERVASRESTAURANTES.Models;
using System.Collections.Generic;

namespace PROYECTO_RESERVASRESTAURANTES.Controllers
{
    public class NosotrosController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var modelo = new Nosotros
            {
                Titulo = "Sobre Nosotros",
                Descripcion = "Mesa Lista es una plataforma dedicada a facilitar la reserva de restaurantes de manera rápida y sencilla. Nuestro compromiso es conectar a los comensales con experiencias gastronómicas inolvidables.",
                Restaurantes = new List<string>
                {
                    "La Mar Cebichería", "Central", "Osaka", "Tanta", "El Hornero",
                    "Isolina", "Maido", "La Lucha", "Panchita", "Astrid y Gastón"
                }
            };

            return View(modelo);
        }

        [HttpPost]
        public IActionResult Index(Nosotros modelo)
        {
            if (ModelState.IsValid)
            {
                // Aquí puedes enviar el mensaje por correo o guardarlo en BD
                TempData["Mensaje"] = "Gracias por contactarnos. Te responderemos pronto.";
                return RedirectToAction("Index");
            }

            modelo.Restaurantes = new List<string>
            {
                "La Mar Cebichería", "Central", "Osaka", "Tanta", "El Hornero",
                "Isolina", "Maido", "La Lucha", "Panchita", "Astrid y Gastón"
            };

            return View(modelo);
        }
    }
}
