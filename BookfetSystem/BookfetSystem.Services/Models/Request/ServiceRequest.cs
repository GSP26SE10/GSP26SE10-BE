namespace BookfetSystem.Services.Models.Request
{
    public class ServiceCreateRequest
    {
        public string ServiceName { get; set; }

        public string Description { get; set; }

        public decimal? BasePrice { get; set; }

        public string Status { get; set; }

        public string Img { get; set; }
    }

    public class ServiceUpdateRequest
    {
        public int ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string Description { get; set; }

        public decimal? BasePrice { get; set; }

        public string Status { get; set; }

        public string Img { get; set; }
    }

    public class ServiceFilterRequest
    {
        public int? ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string Status { get; set; }
    }
}