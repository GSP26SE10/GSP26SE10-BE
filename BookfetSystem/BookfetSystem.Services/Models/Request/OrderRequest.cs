using BookfetSystem.Services.Enum;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class OrderCreateRequest
    {
        public int? CustomerId { get; set; }

        public OrderStatus? Status { get; set; }
        public decimal? TotalPrice { get; set; }
    }

    public class OrderUpdateRequest
    {
        public int? CustomerId { get; set; }

        public OrderStatus? Status { get; set; }

        public decimal? TotalPrice { get; set; }
    }

    public class OrderFilterRequest
    {
        public int? OrderId { get; set; }
        public int? CustomerId { get; set; }
        public OrderStatus? Status { get; set; }
    }

    // FOR ORDER CREATE BUSSINESS LOGIC
    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }

        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class UpdateCustomerOrderRequest
    {
        public List<UpdateCustomerOrderItemRequest> Items { get; set; } = new();
    }

    public class UpdateCustomerOrderItemRequest
    {
        public int MenuId { get; set; }

        public int PartyCategoryId { get; set; }

        public int NumberOfGuests { get; set; }

        public string Address { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? NoteOrderDetail { get; set; }

        public List<ServiceItemRequest>? Services { get; set; }
    }

    public class CreateOrderItemRequest
    {
        public int MenuId { get; set; }

        public int PartyCategoryId { get; set; }

        public int NumberOfGuests { get; set; }

        public string Address { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public List<ServiceItemRequest>? Services { get; set; }

        public List<CustomDishItemRequest>? CustomDishes { get; set; }
    }

    public class ServiceItemRequest
    {
        public int ServiceId { get; set; }

        public int Quantity { get; set; }
    }

    public class CustomDishItemRequest
    {
        public int DishId { get; set; }
    }

    public class AssignOrderStaffGroupRequest
    {
        [Required(ErrorMessage = "StaffGroupId is required.")]
        public int StaffGroupId { get; set; }
    }

    public class ReviewOrderRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        public int Status { get; set; }
    }
}
