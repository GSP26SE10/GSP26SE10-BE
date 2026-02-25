namespace BookfetSystem.Services.Models.Response
{
    public class MenuDishResponse
    {
        public int MenuDishId { get; set; }
        public int? MenuId { get; set; }
        public int? DishId { get; set; }
        public string MenuName { get; set; }
        public string DishName { get; set; }
    }
}
