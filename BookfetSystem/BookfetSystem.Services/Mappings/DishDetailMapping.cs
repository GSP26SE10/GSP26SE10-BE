using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class DishDetailMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<DishDetailFilterRequest, DishDetail>()
                  .IgnoreNullValues(true);

            config.NewConfig<DishDetail, DishDetailResponse>()
                  .Map(dest => dest.DishName,
                       src => src.Dish != null ? src.Dish.DishName : null)
                  .Map(dest => dest.IngredientName,
                       src => src.Ingredient != null ? src.Ingredient.IngredientName : null);
        }
    }
}
