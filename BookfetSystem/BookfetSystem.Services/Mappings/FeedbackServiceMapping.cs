using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class FeedbackServiceMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<FeedbackServiceFilterRequest, FeedbackService>()
                  .IgnoreNullValues(true)
                  .Ignore(dest => dest.Status);

            config.NewConfig<FeedbackService, FeedbackServiceResponse>()
                  .Map(dest => dest.ServiceName,
                       src => src.Service != null ? src.Service.ServiceName : null)
                  .Map(dest => dest.CustomerName,
                       src => src.Customer != null ? src.Customer.FullName : null)
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<FeedbackServiceStatus>(src.Status));
        }
    }
}
