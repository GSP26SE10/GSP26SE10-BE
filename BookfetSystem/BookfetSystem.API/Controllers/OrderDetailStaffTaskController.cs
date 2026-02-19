using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookfetSystem.API.Controllers
{
    [Route("api/order-detail-staff-task")]
    [ApiController]
    public class OrderDetailStaffTaskController : ControllerBase
    {
        private readonly IOrderDetailStaffTaskService _orderDetailStaffTaskService;

        public OrderDetailStaffTaskController(IOrderDetailStaffTaskService orderDetailStaffTaskService)
        {
            _orderDetailStaffTaskService = orderDetailStaffTaskService;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllOrderDetailStaffTasksFiltered([FromQuery] OrderDetailStaffTaskFilterRequest filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var tasks = await _orderDetailStaffTaskService.GetAllOrderDetailStaffTaskFilteredAsync(filter, page, pageSize);
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrderDetailStaffTask([FromBody] OrderDetailStaffTaskCreateRequest request)
        {
            var result = await _orderDetailStaffTaskService.CreateAsync(request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderDetailStaffTask(int id, [FromBody] OrderDetailStaffTaskUpdateRequest request)
        {
            var result = await _orderDetailStaffTaskService.UpdateAsync(id, request);
            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderDetailStaffTask(int id)
        {
            var result = await _orderDetailStaffTaskService.DeleteAsync(id);
            if (result.Success)
            {
                return NoContent();
            }

            return NotFound(result);
        }
    }
}
