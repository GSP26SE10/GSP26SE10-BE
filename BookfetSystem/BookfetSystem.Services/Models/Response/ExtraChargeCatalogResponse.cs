namespace BookfetSystem.Services.Models.Response
{
    public class ExtraChargeCatalogResponse
    {
        public int ExtraChargeCatalogId { get; set; }
        public string? ChargeType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Status { get; set; }
    }
}
