using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BookfetSystem.Services.Mappings
{
    public class OrderDetailStaffTaskMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<OrderDetailStaffTaskFilterRequest, OrderDetailStaffTask>()
                  .IgnoreNullValues(true)
                  .Map(dest => dest.TaskStatus, src => src.TaskStatus.HasValue ? src.TaskStatus.Value.ToString() : null);

            config.NewConfig<OrderDetailStaffTask, OrderDetailStaffTaskResponse>()
                  .Map(dest => dest.StaffName,
                       src => src.Staff != null ? src.Staff.FullName : null)
                  .Map(dest => dest.TaskStatus,
                       src => EnumHelper.TryParseToInt<StaffTaskStatus>(src.TaskStatus));

            config.NewConfig<OrderDetailStaffTask, StaffMyTaskResponse>()
                  .Map(dest => dest.OrderDetail,
                       src => new StaffMyTaskOrderDetailResponse
                       {
                           OrderDetailId = src.OrderDetail.OrderDetailId,
                           MenuName = src.OrderDetail.Menu != null ? src.OrderDetail.Menu.MenuName : null,
                           MenuImage = GetFirstMenuImage(src.OrderDetail.Menu != null ? src.OrderDetail.Menu.ImgUrl : null),
                           PartyCategory = src.OrderDetail.PartyCategory != null ? src.OrderDetail.PartyCategory.PartyCategoryName : null,
                           NumberOfGuests = src.OrderDetail.NumberOfGuests,
                           Address = src.OrderDetail.Address,
                           StartTime = src.OrderDetail.StartTime,
                           EndTime = src.OrderDetail.EndTime,
                           Status = src.OrderDetail.Status != null ? int.Parse(src.OrderDetail.Status) : null
                       });
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
