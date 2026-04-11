using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class StaffMyTaskMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderDetailStaffTask, StaffMyTaskResponse>()
                  .Map(dest => dest.TaskStartTime, src => src.StartTime)
                  .Map(dest => dest.TaskEndTime, src => src.EndTime)
                  .Map(dest => dest.TaskStatus,
                       src => EnumHelper.TryParseToInt<StaffTaskStatus>(src.TaskStatus))
                  .Map(dest => dest.OrderDetail, src => src.OrderDetail);

            config.NewConfig<OrderDetail, StaffMyTaskOrderDetailResponse>()
                  .Map(dest => dest.MenuId,
                       src => src.MenuId)
                  .Map(dest => dest.MenuName,
                       src => src.Menu != null ? src.Menu.MenuName : null)
                  .Map(dest => dest.PartyCategory,
                       src => src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null)
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<OrderStatus>(src.Status));
        }
    }
}
