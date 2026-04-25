using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class GuestDiscountTierMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<GuestDiscountTierFilterRequest, GuestDiscountTier>()
                .IgnoreNullValues(true)
                .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

            config.NewConfig<GuestDiscountTier, GuestDiscountTierResponse>()
                .Map(dest => dest.Status, src => EnumHelper.TryParseToInt<GuestDiscountTierStatus>(src.Status));
        }
    }
}
