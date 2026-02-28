using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/dish-detail")]
    [ApiController]
    public class DishDetailController : ControllerBase
    {
        private readonly IDishDetailService _dishDetailService;

        public DishDetailController(IDishDetailService dishDetailService)
        {
            _dishDetailService = dishDetailService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllDishDetailsFiltered([FromQuery] DishDetailFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var dishDetails = await _dishDetailService.GetAllDishDetailFilteredAsync(filter, page, pageSize);
            return Ok(dishDetails);
        }

        [HttpPost]
        public async Task<ActionResult> CreateDishDetail([FromBody] DishDetailCreateRequest request)
        {
            var result = await _dishDetailService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDishDetail(int id, [FromBody] DishDetailUpdateRequest request)
        {
            var result = await _dishDetailService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDishDetail(int id)
        {
            var result = await _dishDetailService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
