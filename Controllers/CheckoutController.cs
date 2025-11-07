using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using appReservas.Data;
using appReservas.Models;

namespace appReservas.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StripeSettings _stripeSettings;

        public CheckoutController(ApplicationDbContext context, IOptions<StripeSettings> stripeSettings)
        {
            _context = context;
            _stripeSettings = stripeSettings.Value;
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        // ✅ Crear sesión de pago para una reserva específica
        [HttpPost]
        public IActionResult CreateCheckoutSession(int reservaId)
        {
            try
            {
                var reserva = _context.Reservas.FirstOrDefault(r => r.Id == reservaId);
                if (reserva == null)
                    return NotFound("Reserva no encontrada.");

                // 💰 Obtiene el precio real de la reserva si lo tienes guardado (sino usa uno fijo)
                var monto = reserva.Precio != null ? (int)(reserva.Precio * 100) : 2000; // en centavos

                var domain = $"{Request.Scheme}://{Request.Host}";

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = monto,
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = "Reserva en restaurante",
                                    Description = $"Reserva N° {reserva.Id}"
                                }
                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = $"{domain}/Checkout/Success?reservaId={reserva.Id}",
                    CancelUrl = $"{domain}/Checkout/Cancel"
                };

                var service = new SessionService();
                var session = service.Create(options);

                // Guarda temporalmente el ID de sesión en la reserva si lo necesitas
                reserva.StripeSessionId = session.Id;
                _context.SaveChanges();

                // 🔁 Redirige al checkout de Stripe
                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al crear sesión de pago: {ex.Message}");
            }
        }

        // ✅ Página de pago exitoso
        public IActionResult Success(int reservaId)
        {
            var reserva = _context.Reservas.FirstOrDefault(r => r.Id == reservaId);
            if (reserva != null)
            {
                reserva.EstadoPago = "Pagado";
                _context.SaveChanges();
            }

            ViewBag.ReservaId = reservaId;
            return View();
        }

        // ❌ Página de cancelación
        public IActionResult Cancel()
        {
            return View();
        }
    }
}
