using System.Text.Json.Serialization;

namespace BookfetSystem.Services.Models
{
    /// <summary>
    /// Menu snapshot at order creation (matches DB sample: menuName, basePrice, imgUrl[], dishes[], capturedAt)
    /// </summary>
    public class MenuSnapshotDto
    {
        [JsonPropertyName("menuName")]
        public string? MenuName { get; set; }

        [JsonPropertyName("basePrice")]
        public decimal? BasePrice { get; set; }

        [JsonPropertyName("imgUrl")]
        public object? ImgUrl { get; set; }

        [JsonPropertyName("dishes")]
        public List<DishSnapshotDto> Dishes { get; set; } = new();

        [JsonPropertyName("capturedAt")]
        public string? CapturedAt { get; set; }
    }

    public class DishSnapshotDto
    {
        [JsonPropertyName("dishId")]
        public int DishId { get; set; }

        [JsonPropertyName("dishName")]
        public string? DishName { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
    }

    /// <summary>
    /// Service snapshot at order creation (matches DB sample: services[], capturedAt)
    /// </summary>
    public class ServiceSnapshotDto
    {
        [JsonPropertyName("services")]
        public List<ServiceItemSnapshotDto> Services { get; set; } = new();

        [JsonPropertyName("capturedAt")]
        public string? CapturedAt { get; set; }
    }

    public class ServiceItemSnapshotDto
    {
        [JsonPropertyName("serviceId")]
        public int ServiceId { get; set; }

        [JsonPropertyName("serviceName")]
        public string? ServiceName { get; set; }

        [JsonPropertyName("basePrice")]
        public decimal? BasePrice { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("img")]
        public string? Img { get; set; }
    }

    /// <summary>
    /// Custom dish snapshot at order creation (customDishes[], capturedAt)
    /// </summary>
    public class CustomDishSnapshotDto
    {
        [JsonPropertyName("customDishes")]
        public List<CustomDishItemSnapshotDto> CustomDishes { get; set; } = new();

        [JsonPropertyName("capturedAt")]
        public string? CapturedAt { get; set; }
    }

    public class CustomDishItemSnapshotDto
    {
        [JsonPropertyName("dishId")]
        public int DishId { get; set; }

        [JsonPropertyName("dishName")]
        public string? DishName { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal? UnitPrice { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal? TotalAmount { get; set; }

        [JsonPropertyName("img")]
        public string? Img { get; set; }
    }

    /// <summary>
    /// Guest discount snapshot captured at order detail creation/update.
    /// </summary>
    public class GuestDiscountSnapshotDto
    {
        [JsonPropertyName("guestDiscountTierId")]
        public int GuestDiscountTierId { get; set; }

        [JsonPropertyName("minGuestCount")]
        public int MinGuestCount { get; set; }

        [JsonPropertyName("actualGuestCount")]
        public int ActualGuestCount { get; set; }

        [JsonPropertyName("discountPercent")]
        public decimal DiscountPercent { get; set; }

        [JsonPropertyName("baseAmount")]
        public decimal BaseAmount { get; set; }

        [JsonPropertyName("discountAmount")]
        public decimal DiscountAmount { get; set; }

        [JsonPropertyName("finalAmount")]
        public decimal FinalAmount { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("capturedAt")]
        public string? CapturedAt { get; set; }
    }

    /// <summary>
    /// Extra charge snapshot captured from order detail extra charges.
    /// </summary>
    public class ExtraChargeSnapshotDto
    {
        [JsonPropertyName("extraCharges")]
        public List<ExtraChargeSnapshotItemDto> ExtraCharges { get; set; } = new();

        [JsonPropertyName("capturedAt")]
        public string? CapturedAt { get; set; }
    }

    public class ExtraChargeSnapshotItemDto
    {
        [JsonPropertyName("orderDetailExtraChargeId")]
        public int OrderDetailExtraChargeId { get; set; }

        [JsonPropertyName("extraChargeCatalogId")]
        public int? ExtraChargeCatalogId { get; set; }

        [JsonPropertyName("chargeType")]
        public string? ChargeType { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("unitPrice")]
        public decimal? UnitPrice { get; set; }

        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal? TotalAmount { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("createBy")]
        public int? CreateBy { get; set; }

        [JsonPropertyName("incurredAt")]
        public DateTime? IncurredAt { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("image")]
        public object? Image { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }
    }
}
