using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class DishCategoryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<DishCategoryFilterRequest, DishCategory>()
                  .IgnoreNullValues(true);

            config.NewConfig<DishCategory, DishCategoryResponse>();
        }
    }
}
