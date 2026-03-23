namespace BookfetSystem.Services.Models.Response
{
    public class OrderDetailCustomResponse
    {
        public int OrderDetailCustomId { get; set; }

        public int? OrderDetailId { get; set; }

        public int? DishId { get; set; }

        public decimal? TotalAmount { get; set; }

        public string DishName { get; set; }
    }
}