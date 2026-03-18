using System;

namespace BookfetSystem.Services.Models.Response
{
    public class OrderDetailExtraChargeResponse
    {
        public int OrderDetailExtraChargeId { get; set; }
        public int? OrderDetailId { get; set; }
        public int? ExtraChargeCatalogId { get; set; }
        public string? ChargeType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        public int? CreateBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime? IncurredAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public object? Image { get; set; }
        public string? Note { get; set; }
    }
}
