using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace PROYECTO_RESERVASRESTAURANTES.Integration.galletafortuna
{
 public class GalletaApiIntegration
    {
        private readonly HttpClient _httpClient;

        public GalletaApiIntegration(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ObtenerMensajeAsync()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://fortune-cookie4.p.rapidapi.com/slack"),
                Headers =
                {
                    { "X-RapidAPI-Key", "2191cd0a2emshaadf9a8a81936fdp12ad70jsn4d182af0524c" },
                    { "X-RapidAPI-Host", "fortune-cookie4.p.rapidapi.com" }
                }
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Error al obtener la galleta de la fortuna");

            var body = await response.Content.ReadAsStringAsync();
            return body;
        }
    }
}