using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using System;
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
                   .Map(dest => dest.TaskName,
                       src => src.TaskTemplate != null ? src.TaskTemplate.TaskName : null)
                  .Map(dest => dest.TaskStatus,
                       src => EnumHelper.TryParseToInt<StaffTaskStatus>(src.TaskStatus));

            config.NewConfig<OrderDetailStaffTask, StaffMyTaskResponse>()
                  .Map(dest => dest.OrderDetail,
                       src => new StaffMyTaskOrderDetailResponse
                       {
                           OrderDetailId = src.OrderDetail.OrderDetailId,
                           MenuName = src.OrderDetail.Menu != null ? src.OrderDetail.Menu.MenuName : null,
                           MenuImage = GetMenuImage(src.OrderDetail),
                           PartyCategory = src.OrderDetail.PartyCategory != null ? src.OrderDetail.PartyCategory.PartyCategoryName : null,
                           NumberOfGuests = src.OrderDetail.NumberOfGuests,
                           Address = src.OrderDetail.Address,
                           StartTime = src.OrderDetail.StartTime,
                           EndTime = src.OrderDetail.EndTime,
                           Status = src.OrderDetail.Status != null ? int.Parse(src.OrderDetail.Status) : null
                       });
        }

        private static string? GetMenuImage(OrderDetail src)
        {
            var menuImage = GetFirstMenuImage(src.Menu != null ? src.Menu.ImgUrl : null);
            if (!string.IsNullOrWhiteSpace(menuImage))
            {
                return menuImage;
            }

            var snapshot = SnapshotParser.TryParseMenuSnapshot(src.MenuSnapshot);
            return GetFirstMenuImageFromSnapshot(snapshot?.ImgUrl);
        }

        private static string? GetFirstMenuImageFromSnapshot(object? imgUrl)
        {
            if (imgUrl == null)
            {
                return null;
            }

            if (imgUrl is string str)
            {
                return GetFirstMenuImage(str);
            }

            if (imgUrl is IEnumerable<string> strList)
            {
                return strList.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
            }

            if (imgUrl is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.String)
                {
                    var value = element.GetString();
                    return string.IsNullOrWhiteSpace(value) ? null : value;
                }

                if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            var value = item.GetString();
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                return value;
                            }
                        }
                    }
                }
            }

            return null;
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
