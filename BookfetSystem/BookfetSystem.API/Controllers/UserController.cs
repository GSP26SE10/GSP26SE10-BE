using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookfetSystem.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsersFiltered([FromQuery] UserFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var accounts = await _userService.GetAllUserFilteredAsync(filter, page, pageSize);
            return Ok(accounts);
        }

        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] UserCreateRequest request)
        {
            var result = await _userService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest request)
        {
            var result = await _userService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
