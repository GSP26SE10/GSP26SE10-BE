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
                  .Map(dest => dest.StaffGroup,
                       src => new StaffGroupAssignmentGroupResponse
                       {
                           StaffGroupId = src.StaffGroupId,
                           StaffGroupName = src.StaffGroupName,
                           Leader = new StaffGroupAssignmentMemberResponse
                           {
                               StaffId = src.LeaderId,
                               StaffName = src.Leader != null ? src.Leader.FullName : null
                           },
                           Members = src.StaffGroupMembers
                               .Where(m => m.StaffId.HasValue && m.Staff != null)
                               .OrderBy(m => m.Staff!.FullName)
                               .Select(m => new StaffGroupAssignmentMemberResponse
                               {
                                   StaffId = m.StaffId,
                                   StaffName = m.Staff != null ? m.Staff.FullName : null
                               })
                               .ToList()
                       })
                  .Map(dest => dest.Orders,
                       src => src.OrderDetails
                           .OrderBy(od => od.StartTime));

            config.NewConfig<OrderDetail, StaffGroupAssignmentOrderResponse>()
                  .Map(dest => dest.OrderId,
                       src => src.OrderId)
                  .Map(dest => dest.Status,
                       src => new StaffGroupAssignmentStatusResponse
                       {
                                 Order = src.Order != null ? EnumHelper.TryParseToInt<OrderStatus>(src.Order.Status) : null,
                                 OrderDetail = EnumHelper.TryParseToInt<OrderDetailStatus>(src.Status)
                       })
                  .Map(dest => dest.Pricing,
                       src => new StaffGroupAssignmentPricingResponse
                       {
                           TotalPrice = src.TotalPrice ?? (src.Order != null ? src.Order.TotalPrice : null),
                           DepositAmount = src.Order != null ? src.Order.DepositAmount : null,
                           RemainingAmount = src.Order != null ? src.Order.RemainingAmount : null,
                           ExtraChargeTotal = src.OrderDetailExtraCharges.Sum(ec => ec.TotalAmount) ?? 0
                       })
                  .Map(dest => dest.Customer,
                       src => new StaffGroupAssignmentCustomerResponse
                       {
                           Name = src.Order != null && src.Order.Customer != null ? src.Order.Customer.FullName : null,
                           Phone = src.Order != null && src.Order.Customer != null ? src.Order.Customer.Phone : null
                       })
                  .Map(dest => dest.Menu,
                       src => new StaffGroupAssignmentMenuResponse
                       {
                           Name = src.Menu != null ? src.Menu.MenuName : null,
                           Image = GetSnapshotMenuImage(src.MenuSnapshot)
                       })
                  .Map(dest => dest.CustomDishSnapshot,
                       src => SnapshotParser.TryParseCustomDishSnapshot(src.CustomDishSnapshot))
                  .Map(dest => dest.Party,
                       src => new StaffGroupAssignmentPartyResponse
                       {
                           Category = src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null,
                           NumberOfGuests = src.NumberOfGuests
                       })
                  .Map(dest => dest.Schedule,
                       src => new StaffGroupAssignmentScheduleResponse
                       {
                           Address = src.Address,
                           StartTime = src.StartTime,
                           EndTime = src.EndTime
                       })
                  .Map(dest => dest.ExtraCharges,
                       src => src.OrderDetailExtraCharges.OrderByDescending(ec => ec.CreatedAt))
                  .Map(dest => dest.Tasks,
                       src => src.OrderDetailStaffTasks.OrderBy(t => t.TaskId));

            config.NewConfig<OrderDetailStaffTask, StaffGroupAssignmentTaskResponse>()
               .Map(dest => dest.TaskName,
                    src => src.TaskName)
               .Map(dest => dest.Assignees,
                    src => GetAssignees(src))
               .Map(dest => dest.StartTime,
                    src => src.StartTime)
               .Map(dest => dest.EndTime,
                    src => src.EndTime)
                  .Map(dest => dest.Status,
                        src => EnumHelper.TryParseToInt<StaffTaskStatus>(src.TaskStatus))
               .Map(dest => dest.Note, src => src.Note);

            config.NewConfig<OrderDetailExtraCharge, StaffGroupAssignmentExtraChargeResponse>()
                .Map(dest => dest.Id,
                     src => src.OrderDetailExtraChargeId)
                .Map(dest => dest.Type,
                     src => src.ChargeType)
                .Map(dest => dest.CreatedBy,
                     src => new StaffGroupAssignmentExtraChargeCreatorResponse
                     {
                         Id = src.CreateBy,
                         Name = src.CreateByNavigation != null ? src.CreateByNavigation.FullName : null
                     })
                .Map(dest => dest.Images,
                     src => GetImages(src.Image));
        }

          private static List<StaffGroupAssignmentTaskAssigneeResponse> GetAssignees(OrderDetailStaffTask src)
          {
               if (!src.StaffId.HasValue)
               {
                    return new List<StaffGroupAssignmentTaskAssigneeResponse>();
               }

               return new List<StaffGroupAssignmentTaskAssigneeResponse>
               {
                    new StaffGroupAssignmentTaskAssigneeResponse
                    {
                         StaffId = src.StaffId,
                         StaffName = src.Staff != null ? src.Staff.FullName : null
                    }
               };
          }

          private static List<string> GetImages(string? rawImage)
          {
               if (string.IsNullOrWhiteSpace(rawImage))
               {
                    return new List<string>();
               }

               var parsed = SnapshotParser.TryParseJsonToObject(rawImage);
               if (parsed is JsonElement element)
               {
                    if (element.ValueKind == JsonValueKind.String)
                    {
                         var value = element.GetString();
                         return string.IsNullOrWhiteSpace(value)
                              ? new List<string>()
                              : new List<string> { value };
                    }

                    if (element.ValueKind == JsonValueKind.Array)
                    {
                         var images = element.EnumerateArray()
                             .Where(x => x.ValueKind == JsonValueKind.String)
                             .Select(x => x.GetString())
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Select(x => x!)
                             .ToList();
                         return images;
                    }
               }

               var first = GetFirstMenuImage(rawImage);
               return string.IsNullOrWhiteSpace(first)
                    ? new List<string>()
                    : new List<string> { first };
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
