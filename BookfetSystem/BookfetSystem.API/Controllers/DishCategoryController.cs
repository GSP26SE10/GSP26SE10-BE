using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/dish-category")]
    [ApiController]
    public class DishCategoryController : ControllerBase
    {
        private readonly IDishCategoryService _dishCategoryService;

        public DishCategoryController(IDishCategoryService dishCategoryService)
        {
            _dishCategoryService = dishCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllDishCategoriesFiltered([FromQuery] DishCategoryFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var dishCategories = await _dishCategoryService.GetAllDishCategoryFilteredAsync(filter, page, pageSize);
            return Ok(dishCategories);
        }

        [HttpPost]
        public async Task<ActionResult> CreateDishCategory([FromBody] DishCategoryCreateRequest request)
        {
            var result = await _dishCategoryService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDishCategory(int id, [FromBody] DishCategoryUpdateRequest request)
        {
            var result = await _dishCategoryService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDishCategory(int id)
        {
            var result = await _dishCategoryService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
