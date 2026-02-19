using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/message")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllMessagesFiltered([FromQuery] MessageFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var messages = await _messageService.GetAllMessageFilteredAsync(filter, page, pageSize);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult> CreateMessage([FromBody] MessageCreateRequest request)
        {
            var result = await _messageService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMessage(int id, [FromBody] MessageUpdateRequest request)
        {
            var result = await _messageService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var result = await _messageService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
