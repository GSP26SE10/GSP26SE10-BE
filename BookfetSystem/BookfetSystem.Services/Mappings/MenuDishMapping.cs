using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class MenuDishMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MenuDishFilterRequest, MenuDish>()
                  .IgnoreNullValues(true);

            config.NewConfig<MenuDish, MenuDishResponse>()
                  .Map(dest => dest.MenuName,
                       src => src.Menu != null ? src.Menu.MenuName : null)
                  .Map(dest => dest.DishName,
                       src => src.Dish != null ? src.Dish.DishName : null);
        }
    }
}
