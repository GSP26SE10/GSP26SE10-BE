using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class MenuCategoryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MenuCategoryFilterRequest, MenuCategory>()
                  .IgnoreNullValues(true);

            config.NewConfig<MenuCategory, MenuCategoryResponse>();
        }
    }
}
