using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Response;
using Mapster;
using System.Linq;

namespace BookfetSystem.Services.Mappings
{
    public class StaffGroupAssignmentOverviewMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<StaffGroup, StaffGroupAssignmentOverviewResponse>()
                  .Map(dest => dest.LeaderId,
                       src => src.LeaderId)
                  .Map(dest => dest.LeaderName,
                       src => src.Leader != null ? src.Leader.FullName : null)
                  .Map(dest => dest.Members,
                       src => src.StaffGroupMembers
                           .Where(m => m.StaffId.HasValue && m.Staff != null)
                           .OrderBy(m => m.Staff!.FullName))
                  .Map(dest => dest.Orders,
                       src => src.OrderDetails
                           .OrderBy(od => od.StartTime));

            config.NewConfig<StaffGroupMember, StaffGroupAssignmentMemberResponse>()
                  .Map(dest => dest.StaffName,
                       src => src.Staff != null ? src.Staff.FullName : null);

            config.NewConfig<OrderDetail, StaffGroupAssignmentOrderResponse>()
                  .Map(dest => dest.MenuId,
                       src => src.MenuId)
                  .Map(dest => dest.PartyCategory,
                       src => src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null)
                  .Map(dest => dest.MenuName,
                       src => src.Menu != null ? src.Menu.MenuName : null)
                  .Map(dest => dest.Tasks,
                       src => src.OrderDetailStaffTasks.OrderBy(t => t.TaskId));

            config.NewConfig<OrderDetailStaffTask, StaffGroupAssignmentTaskResponse>()
                  .Map(dest => dest.Status,
                            src => src.TaskStatus);
        }
    }
}
