using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class OrderDetailMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderDetailFilterRequest, OrderDetail>()
                .IgnoreNullValues(true);

            config.NewConfig<OrderDetail, OrderDetailResponse>()
                .Map(dest => dest.MenuName,
                    src => src.Menu != null ? src.Menu.MenuName : null)
                .Map(dest => dest.PartyCategoryName,
                    src => src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null);
        }
    }
}
