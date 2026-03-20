using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Models.Response;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BookfetSystem.Services.Mappings
{
    public class StaffGroupAssignmentOverviewMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<StaffGroup, StaffGroupAssignmentOverviewResponse>()
                  .Map(dest => dest.LeaderId,
                       src => src.LeaderId)
                  .Map(dest => dest.LeaderName,
                       src => src.Leader != null ? src.Leader.FullName : null)
                  .Map(dest => dest.Members,
                       src => src.StaffGroupMembers
                           .Where(m => m.StaffId.HasValue && m.Staff != null)
                           .OrderBy(m => m.Staff!.FullName))
                  .Map(dest => dest.Orders,
                       src => src.OrderDetails
                           .OrderBy(od => od.StartTime));

            config.NewConfig<StaffGroupMember, StaffGroupAssignmentMemberResponse>()
                  .Map(dest => dest.StaffName,
                       src => src.Staff != null ? src.Staff.FullName : null);

            config.NewConfig<OrderDetail, StaffGroupAssignmentOrderResponse>()
                  .Map(dest => dest.OrderStatus,
                       src => EnumHelper.TryParseToInt<OrderStatus>(src.Status))
                  .Map(dest => dest.CustomerName,
                       src => src.Order != null && src.Order.Customer != null ? src.Order.Customer.FullName : null)
                  .Map(dest => dest.CustomerPhone,
                       src => src.Order != null && src.Order.Customer != null ? src.Order.Customer.Phone : null)
                  .Map(dest => dest.TotalPrice,
                       src => src.TotalPrice ?? (src.Order != null ? src.Order.TotalPrice : null))
                  .Map(dest => dest.DepositAmount,
                       src => src.Order != null ? src.Order.DepositAmount : null)
                  .Map(dest => dest.RemainingAmount,
                       src => src.Order != null ? src.Order.RemainingAmount : null)
                  .Map(dest => dest.PartyCategory,
                       src => src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null)
                  .Map(dest => dest.MenuName,
                       src => src.Menu != null ? src.Menu.MenuName : null)
                  .Map(dest => dest.MenuImage,
                       src => GetSnapshotMenuImage(src.MenuSnapshot))
                  .Map(dest => dest.Tasks,
                       src => src.OrderDetailStaffTasks.OrderBy(t => t.TaskId));

            config.NewConfig<OrderDetailStaffTask, StaffGroupAssignmentTaskResponse>()
               .Map(dest => dest.AssigneeId,
                    src => src.StaffId)
               .Map(dest => dest.AssigneeName,
                    src => src.Staff != null ? src.Staff.FullName : null)
               .Map(dest => dest.StartTime,
                    src => src.StartTime)
               .Map(dest => dest.EndTime,
                    src => src.EndTime)
                  .Map(dest => dest.Status,
                        src => EnumHelper.TryParseToInt<StaffTaskStatus>(src.TaskStatus))
               .Map(dest => dest.Note, src => src.Note);
        }

          private static string? GetSnapshotMenuImage(string? menuSnapshotJson)
          {
               var snapshot = SnapshotParser.TryParseMenuSnapshot(menuSnapshotJson);
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
                    // Fallback below when ImgUrl is not valid JSON.
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
