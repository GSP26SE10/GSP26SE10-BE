using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class StaffGroupMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<StaffGroupFilterRequest, StaffGroup>()
                  .IgnoreNullValues(true);

            config.NewConfig<StaffGroup, StaffGroupResponse>()
                  .Map(dest => dest.LeaderName,
                       src => src.Leader != null ? src.Leader.FullName : null);
        }
    }
}

