using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/role")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllRolesFiltered([FromQuery] RoleFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var roles = await _roleService.GetAllRoleFilteredAsync(filter, page, pageSize);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateRole([FromBody] RoleCreateRequest request)
        {
            try
            {
                var result = await _roleService.CreateAsync(request);
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(int id, [FromBody] RoleUpdateRequest request)
        {
            try
            {
                var result = await _roleService.UpdateAsync(id, request);
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRole(int id)
        {
            try
            {
                var result = await _roleService.DeleteAsync(id);
                if (result.Success)
                {
                    return NoContent();
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

