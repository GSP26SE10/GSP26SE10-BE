using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly AISuggestionHandler _handler;

        public AIController(AISuggestionHandler handler)
        {
            _handler = handler;
        }

        [HttpPost("suggest-menu")]
        public async Task<IActionResult> SuggestMenu(MenuSuggestionRequest request)
        {
            var result = await _handler.Handle(request);
            return Ok(result);
        }
    }
}
