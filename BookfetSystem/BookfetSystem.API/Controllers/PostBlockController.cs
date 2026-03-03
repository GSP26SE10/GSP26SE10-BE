using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/post-block")]
    [ApiController]
    public class PostBlockController : ControllerBase
    {
        private readonly IPostBlockService _postBlockService;

        public PostBlockController(IPostBlockService postBlockService)
        {
            _postBlockService = postBlockService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPostBlocksFiltered([FromQuery] PostBlockFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _postBlockService.GetAllPostBlockFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreatePostBlock([FromBody] PostBlockCreateRequest request)
        {
            var result = await _postBlockService.CreateAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePostBlock(int id, [FromBody] PostBlockUpdateRequest request)
        {
            var result = await _postBlockService.UpdateAsync(id, request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePostBlock(int id)
        {
            var result = await _postBlockService.DeleteAsync(id);
            if (result.Success)
                return NoContent();
            return NotFound(result);
        }
    }
}
