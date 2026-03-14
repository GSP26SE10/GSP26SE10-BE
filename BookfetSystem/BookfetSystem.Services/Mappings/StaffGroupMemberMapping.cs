using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class StaffGroupMemberMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<StaffGroupMemberFilterRequest, StaffGroupMember>()
                  .IgnoreNullValues(true);

            config.NewConfig<StaffGroupMember, StaffGroupMemberResponse>()
                  .Map(dest => dest.StaffName,
                       src => src.Staff != null ? src.Staff.FullName : null)
                  .Map(dest => dest.StaffGroupName,
                       src => src.StaffGroup != null ? src.StaffGroup.StaffGroupName : null)
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<StaffGroupStatus>(src.Status));
        }
    }
}
