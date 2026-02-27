using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class DishMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<DishFilterRequest, Dish>()
                  .IgnoreNullValues(true);

            config.NewConfig<Dish, DishResponse>()
                  .Map(dest => dest.DishCategoryName,
                       src => src.DishCategory != null ? src.DishCategory.DishCategoryName : null);
        }
    }
}
