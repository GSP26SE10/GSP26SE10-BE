using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class ServiceMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ServiceFilterRequest,Repositories.Entities.Service>()
                .IgnoreNullValues(true);

            // Map from repository entity to response DTO.
            config.NewConfig<Repositories.Entities.Service, ServiceResponse>();
        }
    }
}
