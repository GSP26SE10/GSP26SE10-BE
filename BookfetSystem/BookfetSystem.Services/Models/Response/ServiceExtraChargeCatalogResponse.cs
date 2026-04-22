namespace BookfetSystem.Services.Models.Response
{
    public class ServiceExtraChargeCatalogResponse
    {
        public int ServiceExtraChargeCatalogId { get; set; }
        public int ServiceId { get; set; }
        public int ExtraChargeCatalogId { get; set; }
        public string? ServiceName { get; set; }
        public string? ExtraChargeCatalogTitle { get; set; }
        public string? ExtraChargeType { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
