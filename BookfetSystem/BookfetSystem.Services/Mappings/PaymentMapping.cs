using BookfetSystem.Repositories.Entities;
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

            config.NewConfig<Payment, PaymentResponse>();
        }
    }
}
