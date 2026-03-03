using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class PostBlockMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PostBlockFilterRequest, PostBlock>()
                .IgnoreNullValues(true);
        }
    }
}
