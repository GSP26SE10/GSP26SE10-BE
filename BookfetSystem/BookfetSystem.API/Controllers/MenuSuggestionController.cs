using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [ApiController]
    [Route("api/menu-suggestion")]
    public class MenuSuggestionController : ControllerBase
    {
        private readonly IMenuSuggestionService _menuSuggestionService;

        public MenuSuggestionController(IMenuSuggestionService service)
        {
            _menuSuggestionService = service;
        }

        [HttpPost("ai-suggest")]
        public async Task<IActionResult> SuggestMenu([FromBody] MenuSuggestionRequest request)
        {
            var result = await _menuSuggestionService.SuggestMenu(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return StatusCode(result.Code ?? 500, result);
        }
    }
}
