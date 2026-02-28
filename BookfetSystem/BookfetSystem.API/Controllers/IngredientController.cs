using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/ingredient")]
    [ApiController]
    public class IngredientController : ControllerBase
    {
        private readonly IIngredientService _ingredientService;

        public IngredientController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllIngredientsFiltered([FromQuery] IngredientFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var ingredients = await _ingredientService.GetAllIngredientFilteredAsync(filter, page, pageSize);
            return Ok(ingredients);
        }

        [HttpPost]
        public async Task<ActionResult> CreateIngredient([FromBody] IngredientCreateRequest request)
        {
            var result = await _ingredientService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateIngredient(int id, [FromBody] IngredientUpdateRequest request)
        {
            var result = await _ingredientService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteIngredient(int id)
        {
            var result = await _ingredientService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
