using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Helpers;
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
                       src => src.Sender != null ? src.Sender.FullName : null)
                  .Map(dest => dest.MenuName,
                       src => src.Menu != null ? src.Menu.MenuName : null)
                  .Map(dest => dest.MenuPrice,
                       src => src.Menu != null ? src.Menu.BasePrice : null)
                  .Map(dest => dest.MenuImage,
                       src => src.Menu != null ? SnapshotParser.TryParseJsonToObject(src.Menu.ImgUrl) : null);
        }
    }
}
