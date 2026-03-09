using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class OrderMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderFilterRequest, Order>()
                  .IgnoreNullValues(true);

            config.NewConfig<Order, OrderResponse>()
                  .Map(dest => dest.CustomerName,
                       src => src.Customer != null ? src.Customer.FullName : null);
        }
    }
}