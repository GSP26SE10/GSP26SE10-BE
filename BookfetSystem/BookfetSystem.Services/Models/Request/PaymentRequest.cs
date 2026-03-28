using BookfetSystem.Services.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace BookfetSystem.Services.Models.Request
{
    public class PaymentCreateRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public int OrderId { get; set; }

        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "PaymentType is required.")]
        [EnumDataType(typeof(PaymentType), ErrorMessage = "Invalid payment type. Use 1 for DEPOSIT, 2 for FULL.")]
        public PaymentType PaymentType { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Invalid payment method. Use 1 for CASH, 2 for BANK_TRANSFER, 3 for ZALOPAY.")]
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class PaymentUpdateRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public int OrderId { get; set; }

        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "PaymentType is required.")]
        [EnumDataType(typeof(PaymentType), ErrorMessage = "Invalid payment type. Use 1 for DEPOSIT, 2 for FULL.")]
        public PaymentType PaymentType { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Invalid payment method. Use 1 for CASH, 2 for BANK_TRANSFER, 3 for ZALOPAY.")]
        public PaymentMethod PaymentMethod { get; set; }

        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "Invalid payment status. Use 1 for UNPAID, 2 for PAID.")]
        public PaymentStatus? PaymentStatus { get; set; }

        public DateTime? PaidAt { get; set; }
    }

    public class PaymentFilterRequest
    {
        public int PaymentId { get; set; }
        public int? OrderId { get; set; }
        public PaymentType? PaymentType { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
    }
}
