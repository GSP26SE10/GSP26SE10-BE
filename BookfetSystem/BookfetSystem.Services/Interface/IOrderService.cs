using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interface
{
    public interface IOrderService
    {
        Task<PagedResponse<OrderResponse>> GetAllFilteredAsync(OrderFilterRequest request, int page, int pageSize);
        Task<ApiResponse<OrderResponse>> CreateAsync(OrderCreateRequest request);
        Task<ApiResponse<OrderResponse>> UpdateAsync(int id, OrderUpdateRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}