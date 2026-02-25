using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/menu-dish")]
    [ApiController]
    public class MenuDishController : ControllerBase
    {
        private readonly IMenuDishService _menuDishService;

        public MenuDishController(IMenuDishService menuDishService)
        {
            _menuDishService = menuDishService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllMenuDishesFiltered([FromQuery] MenuDishFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var menuDishes = await _menuDishService.GetAllMenuDishFilteredAsync(filter, page, pageSize);
            return Ok(menuDishes);
        }

        [HttpPost]
        public async Task<ActionResult> CreateMenuDish([FromBody] MenuDishCreateRequest request)
        {
            var result = await _menuDishService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateMenuDish(int id, [FromBody] MenuDishUpdateRequest request)
        {
            var result = await _menuDishService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMenuDish(int id)
        {
            var result = await _menuDishService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
