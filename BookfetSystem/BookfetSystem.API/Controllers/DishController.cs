using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/dish")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private readonly IDishService _dishService;

        public DishController(IDishService dishService)
        {
            _dishService = dishService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllDishesFiltered([FromQuery] DishFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var dishes = await _dishService.GetAllDishFilteredAsync(filter, page, pageSize);
            return Ok(dishes);
        }

        [HttpPost]
        public async Task<ActionResult> CreateDish([FromBody] DishCreateRequest request)
        {
            var result = await _dishService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDish(int id, [FromBody] DishUpdateRequest request)
        {
            var result = await _dishService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDish(int id)
        {
            var result = await _dishService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
