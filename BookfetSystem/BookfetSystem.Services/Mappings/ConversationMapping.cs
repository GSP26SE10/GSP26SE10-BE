using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class ConversationMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ConversationFilterRequest, Conversation>()
                  .IgnoreNullValues(true);

            config.NewConfig<Conversation, ConversationResponse>()
                  .Map(dest => dest.CustomerName,
                       src => src.Customer != null ? src.Customer.FullName : null)
                  .Map(dest => dest.OwnerName,
                       src => src.Owner != null ? src.Owner.FullName : null);
        }
    }
}
