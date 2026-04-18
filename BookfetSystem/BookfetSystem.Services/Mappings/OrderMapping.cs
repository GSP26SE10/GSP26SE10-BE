using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
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
                  .IgnoreNullValues(true)
                  .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

            config.NewConfig<Order, OrderResponse>()
                  .Map(dest => dest.CustomerName,
                       src => src.Customer != null ? src.Customer.FullName : null)
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<OrderStatus>(src.Status))
                  .Map(dest => dest.MtdZlp,
                       src => SnapshotParser.TryParseJsonToObject(src.MtdZlp))
                  .Map(dest => dest.OrderDetails,
                       src => src.OrderDetails != null ? src.OrderDetails.Select(od => od.Adapt<OrderDetailResponse>()).ToList() : new List<OrderDetailResponse>());
        }
    }
}