namespace BookfetSystem.Services.Models.Response
{
    public class OrderServiceResponse
    {
        public int OrderServiceId { get; set; }

        public int? OrderDetailId { get; set; }

        public int? ServiceId { get; set; }

        public string ServiceName { get; set; }

        public int? Quantity { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}