using Microsoft.AspNetCore.Mvc;
using PROYECTO_RESERVASRESTAURANTES.Integration.chatbot;

namespace appReservas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatbotService _chatbot;

        public ChatController(ChatbotService chatbot)
        {
            _chatbot = chatbot;
        }

        [HttpPost("message")]
        public async Task<IActionResult> Message([FromBody] ChatRequest req)
        {
            var respuesta = await _chatbot.ObtenerRespuestaAsync(req.Message);
            return Ok(new { reply = respuesta });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
