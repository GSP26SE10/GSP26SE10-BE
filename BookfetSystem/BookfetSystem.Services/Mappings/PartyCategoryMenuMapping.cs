using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class PartyCategoryMenuMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PartyCategoryMenuFilterRequest, PartyCategoryMenu>()
                  .IgnoreNullValues(true);

            config.NewConfig<PartyCategoryMenu, PartyCategoryMenuResponse>()
                  .Map(dest => dest.PartyCategoryName,
                       src => src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null)
                  .Map(dest => dest.ServiceDurationMinutes,
                       src => src.PartyCategory != null ? src.PartyCategory.ServiceDurationMinutes : null)
                  .Map(dest => dest.MenuName,
                       src => src.Menu != null ? src.Menu.MenuName : null);
        }
    }
}
