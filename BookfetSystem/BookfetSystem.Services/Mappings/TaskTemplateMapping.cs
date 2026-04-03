using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class TaskTemplateMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<TaskTemplate, TaskTemplateResponse>();
        }
    }
}
