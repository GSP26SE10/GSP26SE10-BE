using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class PartyCategoryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PartyCategoryFilterRequest, PartyCategory>()
                  .IgnoreNullValues(true);

            config.NewConfig<PartyCategory, PartyCategoryResponse>();
        }
    }
}