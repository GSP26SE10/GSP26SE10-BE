using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;

        public ConversationController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllConversationsFiltered([FromQuery] ConversationFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var conversations = await _conversationService.GetAllConversationFilteredAsync(filter, page, pageSize);
            return Ok(conversations);
        }

        [HttpPost]
        public async Task<ActionResult> CreateConversation([FromBody] ConversationCreateRequest request)
        {
            var result = await _conversationService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateConversation(int id, [FromBody] ConversationUpdateRequest request)
        {
            var result = await _conversationService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteConversation(int id)
        {
            var result = await _conversationService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
