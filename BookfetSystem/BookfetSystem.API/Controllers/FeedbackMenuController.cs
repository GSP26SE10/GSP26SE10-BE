using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/feedback-menu")]
    [ApiController]
    public class FeedbackMenuController : ControllerBase
    {
        private readonly IFeedbackMenuService _feedbackMenuService;

        public FeedbackMenuController(IFeedbackMenuService feedbackMenuService)
        {
            _feedbackMenuService = feedbackMenuService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllFeedbackMenusFiltered([FromQuery] FeedbackMenuFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var feedbackMenus = await _feedbackMenuService.GetAllFeedbackMenuFilteredAsync(filter, page, pageSize);
            return Ok(feedbackMenus);
        }

        [HttpPost]
        public async Task<ActionResult> CreateFeedbackMenu([FromBody] FeedbackMenuCreateRequest request)
        {
            var result = await _feedbackMenuService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateFeedbackMenu(int id, [FromBody] FeedbackMenuUpdateRequest request)
        {
            var result = await _feedbackMenuService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFeedbackMenu(int id)
        {
            var result = await _feedbackMenuService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
