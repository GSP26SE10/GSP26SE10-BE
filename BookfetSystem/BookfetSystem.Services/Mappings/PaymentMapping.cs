using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class PaymentMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PaymentFilterRequest, Payment>()
                  .IgnoreNullValues(true);

            config.NewConfig<Payment, PaymentResponse>()
                  .Map(dest => dest.PaymentType,
                      src => EnumHelper.TryParseToInt<PaymentType>(src.PaymentType))
                  .Map(dest => dest.PaymentMethod,
                      src => EnumHelper.TryParseToInt<PaymentMethod>(src.PaymentMethod))
                  .Map(dest => dest.PaymentStatus,
                      src => EnumHelper.TryParseToInt<PaymentStatus>(src.PaymentStatus));
        }
    }
}
