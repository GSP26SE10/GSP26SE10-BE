using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class PostMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PostFilterRequest, Post>()
                .IgnoreNullValues(true);

            config.NewConfig<Post, PostResponse>()
                .Map(dest => dest.BlogCategoryName, src => src.BlogCategory != null ? src.BlogCategory.Name : null);
        }
    }
}
