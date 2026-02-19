using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class MessageMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MessageFilterRequest, Message>()
                  .IgnoreNullValues(true);

            config.NewConfig<Message, MessageResponse>()
                  .Map(dest => dest.SenderName,
                       src => src.Sender != null ? src.Sender.FullName : null);
        }
    }
}
