using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class ServiceMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ServiceFilterRequest, Repositories.Entities.Service>()
                .IgnoreNullValues(true);

            config.NewConfig<Repositories.Entities.Service, ServiceResponse>()
                .Map(dest => dest.Status,
                    src => EnumHelper.TryParseToInt<ServiceStatus>(src.Status));
        }
    }
}
