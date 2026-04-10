using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models;
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
                       src => src.TaskName)
                  .Map(dest => dest.TaskStatus,
                       src => EnumHelper.TryParseToInt<StaffTaskStatus>(src.TaskStatus));

            config.NewConfig<OrderDetailStaffTask, StaffMyTaskResponse>()
                  .Map(dest => dest.OrderDetail,
                       src => new StaffMyTaskOrderDetailResponse
                       {
                           OrderDetailId = src.OrderDetail.OrderDetailId,
                           MenuName = src.OrderDetail.Menu != null ? src.OrderDetail.Menu.MenuName : null,
                           MenuImage = GetMenuImage(src.OrderDetail),
                           ServiceSnapshot = BuildServiceSnapshot(src.OrderDetail),
                           CustomDishSnapshot = BuildCustomDishSnapshot(src.OrderDetail),
                           PartyCategory = src.OrderDetail.PartyCategory != null ? src.OrderDetail.PartyCategory.PartyCategoryName : null,
                           NumberOfGuests = src.OrderDetail.NumberOfGuests,
                           Address = src.OrderDetail.Address,
                           StartTime = src.OrderDetail.StartTime,
                           EndTime = src.OrderDetail.EndTime,
                           Status = EnumHelper.TryParseToInt<OrderDetailStatus>(src.OrderDetail.Status),
                           OrderStatus = src.OrderDetail.Order != null ? EnumHelper.TryParseToInt<OrderStatus>(src.OrderDetail.Order.Status) : null
                       });
        }

        private static ServiceSnapshotDto? BuildServiceSnapshot(OrderDetail src)
        {
            var parsed = SnapshotParser.TryParseServiceSnapshot(src.ServiceSnapshot);
            if (parsed != null)
            {
                return parsed;
            }

            if (src.OrderServices == null || !src.OrderServices.Any())
            {
                return null;
            }

            var items = src.OrderServices
                .Where(x => x.ServiceId.HasValue && x.Service != null)
                .Select(x => new ServiceItemSnapshotDto
                {
                    ServiceId = x.ServiceId!.Value,
                    ServiceName = x.Service!.ServiceName,
                    BasePrice = x.Service.BasePrice,
                    Quantity = x.Quantity ?? 0,
                    Img = x.Service.Img
                })
                .ToList();

            if (!items.Any())
            {
                return null;
            }

            return new ServiceSnapshotDto
            {
                Services = items,
                CapturedAt = null
            };
        }

        private static CustomDishSnapshotDto? BuildCustomDishSnapshot(OrderDetail src)
        {
            var parsed = SnapshotParser.TryParseCustomDishSnapshot(src.CustomDishSnapshot);
            if (parsed != null)
            {
                return parsed;
            }

            if (src.OrderDetailCustoms == null || !src.OrderDetailCustoms.Any())
            {
                return null;
            }

            var items = src.OrderDetailCustoms
                .Where(x => x.DishId.HasValue && x.Dish != null)
                .Select(x => new CustomDishItemSnapshotDto
                {
                    DishId = x.DishId!.Value,
                    DishName = x.Dish!.DishName,
                    UnitPrice = x.Dish.Price,
                    TotalAmount = x.TotalAmount,
                    Img = x.Dish.Img
                })
                .ToList();

            if (!items.Any())
            {
                return null;
            }

            return new CustomDishSnapshotDto
            {
                CustomDishes = items,
                CapturedAt = null
            };
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
