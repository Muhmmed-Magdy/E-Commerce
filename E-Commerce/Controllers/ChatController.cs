using E_Commerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    public class ChatController : Controller
    {
        private readonly AIChatService _aiChatService;

        public ChatController(AIChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        [HttpPost]
        public async Task<IActionResult> Send(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Message is required."
                });
            }

            try
            {
                var response = await _aiChatService.GetResponseAsync(message);

                return Json(new
                {
                    success = true,
                    message = response
                });
            }
            catch (Exception ex)
            {
                // TEMPORARY: show the actual error
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }
    }
}