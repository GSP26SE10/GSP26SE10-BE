using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class OrderServiceMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderServiceFilterRequest, OrderService>()
                  .IgnoreNullValues(true);

            config.NewConfig<OrderService, OrderServiceResponse>()
                  .Map(dest => dest.ServiceName,
                       src => src.Service != null ? src.Service.ServiceName : null);
        }
    }
}
