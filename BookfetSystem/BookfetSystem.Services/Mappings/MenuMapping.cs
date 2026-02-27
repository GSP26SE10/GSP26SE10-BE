using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class MenuMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MenuFilterRequest, Menu>()
                  .IgnoreNullValues(true);

            config.NewConfig<Menu, MenuResponse>();
        }
    }
}
