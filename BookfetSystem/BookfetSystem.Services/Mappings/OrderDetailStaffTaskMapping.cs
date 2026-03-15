using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class OrderDetailStaffTaskMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderDetailStaffTaskFilterRequest, OrderDetailStaffTask>()
                  .IgnoreNullValues(true)
                  .Map(dest => dest.TaskStatus, src => src.TaskStatus.HasValue ? src.TaskStatus.Value.ToString() : null);

            config.NewConfig<OrderDetailStaffTask, OrderDetailStaffTaskResponse>()
                  .Map(dest => dest.StaffName,
                       src => src.Staff != null ? src.Staff.FullName : null)
                  .Map(dest => dest.TaskStatus,
                       src => EnumHelper.TryParseToInt<StaffTaskStatus>(src.TaskStatus));
        }
    }
}
