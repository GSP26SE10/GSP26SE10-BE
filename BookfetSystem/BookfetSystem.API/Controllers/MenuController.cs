using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/menu")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllMenusFiltered([FromQuery] MenuFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var menus = await _menuService.GetAllMenuFilteredAsync(filter, page, pageSize);
            return Ok(menus);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateMenu([FromForm] MenuCreateRequest request)
        {
            var result = await _menuService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdateMenu(int id, [FromForm] MenuUpdateRequest request)
        {
            var result = await _menuService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMenu(int id)
        {
            var result = await _menuService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}