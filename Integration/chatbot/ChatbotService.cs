using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using appReservas.Data;
using Microsoft.Extensions.Configuration;

namespace PROYECTO_RESERVASRESTAURANTES.Integration.chatbot
{
    public class ChatbotService
    {
        private readonly HttpClient _http;
        private readonly ApplicationDbContext _db;
        private readonly string _apiKey;
        private readonly string _model;

        public ChatbotService(HttpClient http, ApplicationDbContext db, IConfiguration config)
        {
            _http = http;
            _db = db;
            _apiKey = config["Gemini:ApiKey"];
            _model = config["Gemini:Model"];
        }

        public async Task<string> ObtenerRespuestaAsync(string mensajeUsuario)
        {
            if (string.IsNullOrWhiteSpace(mensajeUsuario))
                return "¿Qué deseas saber sobre restaurantes? 😊";

            // Restaurantes desde tu BD
            var restaurantes = _db.Restaurantes
                .OrderByDescending(r => r.Rating)
                .Take(10)
                .Select(r => $"{r.Nombre} | {r.Tipo} | {r.Distrito} | Precio: {r.PrecioPromedio} | Rating: {r.Rating}")
                .ToList();

            var contexto = string.Join("\n", restaurantes);

            // JSON EXACTO que usa Gemini 2.5 Flash
            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = 
$@"Eres el asistente de restaurantes de MESA LISTA.
Usa SOLO esta información real de la base de datos:

{contexto}

Pregunta del usuario: {mensajeUsuario}" }
                        }
                    }
                }
            };

            string json = JsonSerializer.Serialize(requestBody);

            string url =
                $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);

            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"❌ Error Gemini: {raw}";

            try
            {
                var doc = JsonDocument.Parse(raw);

                return doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch
            {
                return $"⚠ Error procesando respuesta: {raw}";
            }
        }
    }
}
