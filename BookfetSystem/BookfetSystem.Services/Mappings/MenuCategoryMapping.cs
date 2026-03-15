using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
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
                  .IgnoreNullValues(true)
                  .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

            config.NewConfig<MenuCategory, MenuCategoryResponse>()
                  .Map(dest => dest.Status,
                      src => EnumHelper.TryParseToInt<MenuStatus>(src.Status));
        }
    }
}
