using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;

namespace BookfetSystem.Services.Mappings
{
    public class NotificationMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<NotificationFilterRequest, Notification>()
                .IgnoreNullValues(true)
                .Map(dest => dest.Type, src => src.Type.HasValue ? ((int)src.Type.Value).ToString() : null)
                .Map(dest => dest.IsRead, src => src.IsRead);

            config.NewConfig<Notification, NotificationResponse>()
                .Map(dest => dest.Body, src => src.Content)
                .Map(dest => dest.Type, src => ParseType(src.Type))
                .Map(dest => dest.IsRead, src => src.IsRead ?? false)
                .Map(dest => dest.IsSent, src => src.IsSent ?? false);
        }

        private static int ParseType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return 0;
            }

            if (int.TryParse(type, out var value))
            {
                return value;
            }

            var enumValue = EnumHelper.TryParseToInt<NotificationType>(type);
            return enumValue ?? 0;
        }
    }
}