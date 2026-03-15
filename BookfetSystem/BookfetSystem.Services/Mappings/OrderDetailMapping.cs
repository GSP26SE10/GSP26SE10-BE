using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
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
                .IgnoreNullValues(true)
                .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null)
                .Map(dest => dest.Type, src => src.Type.HasValue ? src.Type.Value.ToString() : null);

            config.NewConfig<OrderDetail, OrderDetailResponse>()
                .Map(dest => dest.MenuName,
                    src => src.Menu != null ? src.Menu.MenuName : null)
                .Map(dest => dest.PartyCategoryName,
                    src => src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null)
                .Map(dest => dest.MenuSnapshot,
                    src => SnapshotParser.TryParseMenuSnapshot(src.MenuSnapshot))
                .Map(dest => dest.ServiceSnapshot,
                    src => SnapshotParser.TryParseServiceSnapshot(src.ServiceSnapshot))
                .Map(dest => dest.Status,
                    src => EnumHelper.TryParseToInt<OrderStatus>(src.Status))
                .Map(dest => dest.Type,
                    src => EnumHelper.TryParseToInt<OrderDetailType>(src.Type));
        }
    }
}
