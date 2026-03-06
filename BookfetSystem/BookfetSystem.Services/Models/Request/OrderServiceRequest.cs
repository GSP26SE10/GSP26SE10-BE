namespace BookfetSystem.Services.Models.Request
{
    public class OrderServiceCreateRequest
    {
        public int? OrderDetailId { get; set; }

        public int? ServiceId { get; set; }

        public int? Quantity { get; set; }
    }

    public class OrderServiceUpdateRequest
    {
        public int OrderServiceId { get; set; }

        public int? OrderDetailId { get; set; }

        public int? ServiceId { get; set; }

        public int? Quantity { get; set; }
    }

    public class OrderServiceFilterRequest
    {
        public int? OrderServiceId { get; set; }

        public int? OrderDetailId { get; set; }

        public int? ServiceId { get; set; }
    }
}