using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class OrderDetailCustomMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderDetailCustomFilterRequest, OrderDetailCustom>()
                  .IgnoreNullValues(true);

            config.NewConfig<OrderDetailCustom, OrderDetailCustomResponse>()
                  .Map(dest => dest.DishName,
                       src => src.Dish != null ? src.Dish.DishName : null);
        }
    }
}
