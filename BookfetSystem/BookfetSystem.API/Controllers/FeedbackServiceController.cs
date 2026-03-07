using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/feedback-service")]
    [ApiController]
    public class FeedbackServiceController : ControllerBase
    {
        private readonly IFeedbackServiceService _feedbackServiceService;

        public FeedbackServiceController(IFeedbackServiceService feedbackServiceService)
        {
            _feedbackServiceService = feedbackServiceService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllFeedbackServicesFiltered([FromQuery] FeedbackServiceFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var feedbackServices = await _feedbackServiceService.GetAllFeedbackServiceFilteredAsync(filter, page, pageSize);
            return Ok(feedbackServices);
        }

        [HttpPost]
        public async Task<ActionResult> CreateFeedbackService([FromBody] FeedbackServiceCreateRequest request)
        {
            var result = await _feedbackServiceService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateFeedbackService(int id, [FromBody] FeedbackServiceUpdateRequest request)
        {
            var result = await _feedbackServiceService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFeedbackService(int id)
        {
            var result = await _feedbackServiceService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
