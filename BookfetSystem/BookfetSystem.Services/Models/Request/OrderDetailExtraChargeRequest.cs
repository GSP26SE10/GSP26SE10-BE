using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class OrderDetailExtraChargeCreateRequest
    {
        [Required(ErrorMessage = "OrderDetailId is required.")]
        public int OrderDetailId { get; set; }

        [Required(ErrorMessage = "ExtraChargeCatalogId is required.")]
        public int ExtraChargeCatalogId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public DateTime? IncurredAt { get; set; }

        public string? Note { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }
    }

    public class OrderDetailExtraChargeUpdateRequest
    {
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        public DateTime? IncurredAt { get; set; }

        public string? Note { get; set; }

        public List<IFormFile>? ImageFiles { get; set; }
    }
}
