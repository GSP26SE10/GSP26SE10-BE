using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/menu-category")]
    [ApiController]
    public class MenuCategoryController : ControllerBase
    {
        private readonly IMenuCategoryService _menuCategoryService;

        public MenuCategoryController(IMenuCategoryService menuCategoryService)
        {
            _menuCategoryService = menuCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllMenuCategoriesFiltered([FromQuery] MenuCategoryFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var menuCategories = await _menuCategoryService.GetAllMenuCategoryFilteredAsync(filter, page, pageSize);
            return Ok(menuCategories);
        }

        [HttpPost]
        public async Task<ActionResult> CreateMenuCategory([FromBody] MenuCategoryCreateRequest request)
        {
            var result = await _menuCategoryService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMenuCategory(int id, [FromBody] MenuCategoryUpdateRequest request)
        {
            var result = await _menuCategoryService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMenuCategory(int id)
        {
            var result = await _menuCategoryService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
