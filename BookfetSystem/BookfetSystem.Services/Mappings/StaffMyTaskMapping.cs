using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Response;
using Mapster;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BookfetSystem.Services.Mappings
{
    public class StaffMyTaskMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderDetailStaffTask, StaffMyTaskResponse>()
                  .Map(dest => dest.TaskStartTime, src => src.StartTime)
                  .Map(dest => dest.TaskEndTime, src => src.EndTime)
                  .Map(dest => dest.TaskStatus,
                       src => EnumHelper.TryParseToInt<StaffTaskStatus>(src.TaskStatus))
                  .Map(dest => dest.OrderDetail, src => src.OrderDetail);

            config.NewConfig<OrderDetail, StaffMyTaskOrderDetailResponse>()
                  .Map(dest => dest.MenuName,
                       src => src.Menu != null ? src.Menu.MenuName : null)
                      .Map(dest => dest.MenuImage,
                            src => GetFirstMenuImage(src.Menu != null ? src.Menu.ImgUrl : null))
                  .Map(dest => dest.PartyCategory,
                       src => src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null)
                  .Map(dest => dest.Status,
                       src => EnumHelper.TryParseToInt<OrderStatus>(src.Status));
        }

          private static string? GetFirstMenuImage(string? rawImgUrl)
          {
               if (string.IsNullOrWhiteSpace(rawImgUrl))
               {
                    return null;
               }

               var trimmed = rawImgUrl.Trim();

               try
               {
                    if (trimmed.StartsWith("["))
                    {
                         var images = JsonSerializer.Deserialize<List<string>>(trimmed);
                         return images?.FirstOrDefault(img => !string.IsNullOrWhiteSpace(img));
                    }

                    if (trimmed.StartsWith("\""))
                    {
                         var single = JsonSerializer.Deserialize<string>(trimmed);
                         return string.IsNullOrWhiteSpace(single) ? null : single;
                    }
               }
               catch
               {
                    // Keep fallback behavior below when ImgUrl is malformed.
               }

               if (trimmed.Contains(","))
               {
                    return trimmed
                         .Split(',')
                         .Select(s => s.Trim())
                         .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
               }

               return trimmed;
          }
    }
}
