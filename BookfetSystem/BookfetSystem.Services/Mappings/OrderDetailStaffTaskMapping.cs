using BookfetSystem.Repositories.Entities;
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
                  .IgnoreNullValues(true);

            config.NewConfig<OrderDetailStaffTask, OrderDetailStaffTaskResponse>()
                  .Map(dest => dest.StaffName,
                       src => src.Staff != null ? src.Staff.FullName : null);
        }
    }
}
