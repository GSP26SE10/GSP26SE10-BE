using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class ContactRequestMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ContactRequestFilterRequest, ContactRequest>()
                .IgnoreNullValues(true)
                .Map(dest => dest.Status,
                     src => src.Status.HasValue ? src.Status.Value.ToString() : null);

            config.NewConfig<ContactRequest, ContactRequestResponse>()
                .Map(dest => dest.Status,
                     src => EnumHelper.TryParseToInt<ContactRequestStatus>(src.Status))
                .Map(dest => dest.CustomerName,
                     src => src.Customer != null ? src.Customer.FullName : null);
        }
    }
}