using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
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
                  .IgnoreNullValues(true)
                  .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

            config.NewConfig<Dish, DishResponse>()
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<DishStatus>(src.Status))
                  .Map(dest => dest.DishCategoryName,
                       src => src.DishCategory != null ? src.DishCategory.DishCategoryName : null);
        }
    }
}
