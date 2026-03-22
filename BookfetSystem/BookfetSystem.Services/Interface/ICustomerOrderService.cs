using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;

namespace BookfetSystem.Services.Interface
{
    public interface ICustomerOrderService
    {
        Task<PagedResponse<OrderResponse>> GetAllFilteredAsync(OrderFilterRequest filter, int page, int pageSize);

        Task<OrderResponse?> GetById(int id);

        Task<ApiResponse<OrderResponse>> Create(OrderCreateRequest request);

        Task<ApiResponse<OrderResponse>> Update(int id, OrderUpdateRequest request);

        Task<ApiResponse<bool>> Delete(int id);

        Task<ApiResponse<int>> CreateOrderAsync(CreateOrderRequest request);
        Task<ApiResponse<OrderResponse>> UpdateCustomerOrderAsync(int orderId, UpdateCustomerOrderRequest request);

        Task<PagedResponse<OrderResponse>> GetDepositedApprovedForAssignmentAsync(int page, int pageSize);

        Task<ApiResponse<OrderResponse>> AssignOrderToStaffGroupAsync(int orderId, int staffGroupId);
        Task<ApiResponse<OrderResponse>> ReviewOrderAsync(int orderId, int status, int reviewerId);
    }
}