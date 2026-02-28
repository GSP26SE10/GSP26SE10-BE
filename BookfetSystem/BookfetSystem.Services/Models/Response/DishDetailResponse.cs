namespace BookfetSystem.Services.Models.Response
{
    public class DishDetailResponse
    {
        public int DishDetailId { get; set; }
        public int? DishId { get; set; }
        public int? IngredientId { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public string? DishName { get; set; }
        public string? IngredientName { get; set; }
    }
}
