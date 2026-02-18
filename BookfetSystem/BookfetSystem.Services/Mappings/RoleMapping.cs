using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class RoleMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<RoleFilterRequest, Role>()
                  .IgnoreNullValues(true);

            config.NewConfig<Role, RoleResponse>();
        }
    }
}

