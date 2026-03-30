using System.Collections.Generic;

namespace BookfetSystem.Services.Models.Response
{
    public class OwnerTopSellingMenuItemResponse
    {
        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public object? ImgUrl { get; set; }
        public int TotalOrders { get; set; }
        public int TotalGuests { get; set; }
    }

    public class OwnerTopSellingMenuResponse
    {
        public List<OwnerTopSellingMenuItemResponse> Items { get; set; } = new();
    }
}
