using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class MenuMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MenuFilterRequest, Menu>()
                  .IgnoreNullValues(true);

            config.NewConfig<Menu, MenuResponse>()
                  .Map(dest => dest.MenuCategoryName,
                       src => src.MenuCategory != null ? src.MenuCategory.MenuCategoryName : null)
                  .Map(dest => dest.PartyCategoryName,
                       src => src.PartyCategoryMenus
                           .OrderBy(pcm => pcm.PartyCategoryMenuId)
                           .Select(pcm => pcm.PartyCategory != null ? pcm.PartyCategory.PartyCategoryName : null)
                           .FirstOrDefault())
                  .Map(dest => dest.ImgUrl,
                       src => SnapshotParser.TryParseJsonToObject(src.ImgUrl))
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<MenuStatus>(src.Status));
        }
    }
}
