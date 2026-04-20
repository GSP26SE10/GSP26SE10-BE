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
                  .IgnoreNullValues(true)
                  .Map(dest => dest.PaymentType, src => src.PaymentType.HasValue ? src.PaymentType.Value.ToString() : null)
                  .Map(dest => dest.PaymentMethod, src => src.PaymentMethod.HasValue ? src.PaymentMethod.Value.ToString() : null)
                  .Map(dest => dest.PaymentStatus, src => src.PaymentStatus.HasValue ? src.PaymentStatus.Value.ToString() : null);

            config.NewConfig<Payment, PaymentResponse>()
                  .Map(dest => dest.PaymentType,
                      src => EnumHelper.TryParseToInt<PaymentType>(src.PaymentType))
                  .Map(dest => dest.PaymentMethod,
                      src => EnumHelper.TryParseToInt<PaymentMethod>(src.PaymentMethod))
                  .Map(dest => dest.PaymentStatus,
                      src => EnumHelper.TryParseToInt<PaymentStatus>(src.PaymentStatus))
                  .Map(dest => dest.MtdZlp,
                      src => src.Order != null ? src.Order.MtdZlp : null);
        }
    }
}
