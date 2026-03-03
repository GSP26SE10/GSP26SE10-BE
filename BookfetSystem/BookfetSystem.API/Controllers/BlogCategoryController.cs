using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/blog-category")]
    [ApiController]
    public class BlogCategoryController : ControllerBase
    {
        private readonly IBlogCategoryService _blogCategoryService;

        public BlogCategoryController(IBlogCategoryService blogCategoryService)
        {
            _blogCategoryService = blogCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllBlogCategoriesFiltered([FromQuery] BlogCategoryFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _blogCategoryService.GetAllBlogCategoryFilteredAsync(filter, page, pageSize);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateBlogCategory([FromBody] BlogCategoryCreateRequest request)
        {
            var result = await _blogCategoryService.CreateAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateBlogCategory(int id, [FromBody] BlogCategoryUpdateRequest request)
        {
            var result = await _blogCategoryService.UpdateAsync(id, request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBlogCategory(int id)
        {
            var result = await _blogCategoryService.DeleteAsync(id);
            if (result.Success)
                return NoContent();
            return NotFound(result);
        }
    }
}
