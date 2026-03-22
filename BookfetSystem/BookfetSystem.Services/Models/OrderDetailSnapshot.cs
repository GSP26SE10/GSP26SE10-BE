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
}
