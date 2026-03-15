using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class StaffGroupMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Filter: Status enum số -> string để query DB
            config.NewConfig<StaffGroupFilterRequest, StaffGroup>()
                  .IgnoreNullValues(true)
                  .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

            config.NewConfig<StaffGroup, StaffGroupResponse>()
                  .Map(dest => dest.LeaderName,
                       src => src.Leader != null ? src.Leader.FullName : null)
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<StaffGroupStatus>(src.Status));
        }
    }
}

