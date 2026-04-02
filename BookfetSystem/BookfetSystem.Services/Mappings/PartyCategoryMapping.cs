using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class PartyCategoryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PartyCategoryFilterRequest, PartyCategory>()
                  .IgnoreNullValues(true)
                  .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

            config.NewConfig<PartyCategory, PartyCategoryResponse>()
                  .Map(dest => dest.Status,
                      src => EnumHelper.TryParseToInt<PartyCategoryStatus>(src.Status));
        }
    }
}