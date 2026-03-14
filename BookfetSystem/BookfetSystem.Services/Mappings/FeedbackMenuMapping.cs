using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class FeedbackMenuMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<FeedbackMenuFilterRequest, FeedbackMenu>()
                  .IgnoreNullValues(true)
                  .Ignore(dest => dest.Status);

            config.NewConfig<FeedbackMenu, FeedbackMenuResponse>()
                  .Map(dest => dest.MenuName,
                       src => src.Menu != null ? src.Menu.MenuName : null)
                  .Map(dest => dest.CustomerName,
                       src => src.Customer != null ? src.Customer.FullName : null)
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<FeedbackMenuStatus>(src.Status));
        }
    }
}
