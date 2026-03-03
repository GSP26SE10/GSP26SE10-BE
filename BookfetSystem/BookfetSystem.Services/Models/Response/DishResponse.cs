namespace BookfetSystem.Services.Models.Response
{
    public class DishResponse
    {
        public int DishId { get; set; }
        public string? Note { get; set; }
        public string? DishName { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Img { get; set; }
        public int? DishCategoryId { get; set; }
        public string? DishCategoryName { get; set; }
    }
}
