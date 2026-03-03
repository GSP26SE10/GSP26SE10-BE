using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class BlogCategoryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<BlogCategoryFilterRequest, BlogCategory>()
                .IgnoreNullValues(true);

            config.NewConfig<BlogCategory, BlogCategoryResponse>();
        }
    }
}
