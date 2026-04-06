using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllPostsFiltered([FromQuery] PostFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetAllPostFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        //[HttpGet("{id}")]
        //public async Task<ActionResult> GetPostById(int id)
        //{
        //    var result = await _postService.GetByIdAsync(id);
        //    if (result.Success)
        //        return Ok(result);
        //    return NotFound(result);
        //}

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreatePost([FromForm] PostCreateRequest request)
        {
            var result = await _postService.CreateAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdatePost(int id, [FromForm] PostUpdateRequest request)
        {
            var result = await _postService.UpdateAsync(id, request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePost(int id)
        {
            var result = await _postService.DeleteAsync(id);
            if (result.Success)
                return NoContent();
            return NotFound(result);
        }
    }
}
