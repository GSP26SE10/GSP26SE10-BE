using System.Text.Json;
using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Models;
using BookfetSystem.Services.Models.Common;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace BookfetSystem.Services.Services
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly GSP26SE10DBContext _dbContext;
        private readonly OrderRepository _orderRepository;
        private readonly OrderDetailRepository _orderDetailRepository;
        private readonly OrderServiceRepository _orderServiceRepository;
        private readonly ServiceRepository _serviceRepository;
        private readonly OrderDetailCustomRepository _orderDetailCustomRepository;
        private readonly DishRepository _dishRepository;
        private readonly UserRepository _userRepository;
        private readonly StaffGroupRepository _staffGroupRepository;
        private readonly MenuRepository _menuRepository;
        private readonly MenuDishRepository _menuDishRepository;
        private readonly PartyCategoryRepository _partyCategoryRepository;
        private readonly IOrderStatusSchedulerService _orderStatusSchedulerService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public CustomerOrderService(GSP26SE10DBContext dbContext, OrderRepository orderRepository, UserRepository userRepository, OrderDetailRepository orderDetailRepository, OrderServiceRepository orderServiceRepository, ServiceRepository serviceRepository, OrderDetailCustomRepository orderDetailCustomRepository, DishRepository dishRepository, StaffGroupRepository staffGroupRepository, MenuRepository menuRepository, MenuDishRepository menuDishRepository, PartyCategoryRepository partyCategoryRepository, IOrderStatusSchedulerService orderStatusSchedulerService, INotificationService notificationService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _orderDetailRepository = orderDetailRepository;
            _orderServiceRepository = orderServiceRepository;
            _serviceRepository = serviceRepository;
            _orderDetailCustomRepository = orderDetailCustomRepository;
            _dishRepository = dishRepository;
            _staffGroupRepository = staffGroupRepository;
            _menuRepository = menuRepository;
            _menuDishRepository = menuDishRepository;
            _partyCategoryRepository = partyCategoryRepository;
            _orderStatusSchedulerService = orderStatusSchedulerService;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<PagedResponse<OrderResponse>> GetAllFilteredAsync(OrderFilterRequest filter, int page, int pageSize)
        {
            var entityFilter = filter.Adapt<Order>();
            entityFilter.Status = filter.Status?.ToString();

            var query = _orderRepository.GetAllOrderFiltered(entityFilter);
            var totalCount = await query.CountAsync();

            var data = await query
                .ProjectToType<OrderResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            await AttachExtraChargeCostsAsync(data);

            return new PagedResponse<OrderResponse>
            {
                Items = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<OrderResponse?> GetById(int id)
        {
            var entity = await _orderRepository.GetByIdWithRelationAsync(id);

            if (entity == null) return null;

            var response = new OrderResponse
            {
                OrderId = entity.OrderId,
                CustomerId = entity.CustomerId,
                CustomerName = entity.Customer?.FullName,
                Status = EnumHelper.TryParseToInt<OrderStatus>(entity.Status),
                TotalPrice = entity.TotalPrice,
                DepositAmount = entity.DepositAmount,
                RemainingAmount = entity.RemainingAmount,
                NoteOrder = entity.NoteOrder,
                CreatedAt = entity.CreatedAt,
                OrderDetails = entity.OrderDetails?.Select(od => od.Adapt<OrderDetailResponse>()).ToList() ?? new List<OrderDetailResponse>()
            };

            await AttachExtraChargeCostsAsync(new List<OrderResponse> { response });
            return response;
        }

        public async Task<ApiResponse<OrderResponse>> Create(OrderCreateRequest request)
        {
            if (request.CustomerId.HasValue)
            {
                var customer = await _userRepository.GetByIdAsync(request.CustomerId.Value);
                if (customer == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Customer not found.",
                        Data = null
                    };
                }
            }

            var entity = new Order
            {
                CustomerId = request.CustomerId,
                Status = (request.Status ?? OrderStatus.PENDING).ToString(),
                TotalPrice = request.TotalPrice,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _orderRepository.CreateAsync(entity);
                var created = await GetById(entity.OrderId);
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order created successfully.",
                    Data = created
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Create order failed.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<OrderResponse>> Update(int id, OrderUpdateRequest request)
        {
            var entity = await _orderRepository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            if (request.CustomerId.HasValue)
            {
                var customer = await _userRepository.GetByIdAsync(request.CustomerId.Value);
                if (customer == null)
                {
                    return new ApiResponse<OrderResponse>
                    {
                        Success = false,
                        Message = "Customer not found.",
                        Data = null
                    };
                }

                entity.CustomerId = request.CustomerId;
            }

            if (request.Status.HasValue)
            {
                entity.Status = request.Status.Value.ToString();
            }

            if (request.TotalPrice.HasValue)
            {
                entity.TotalPrice = request.TotalPrice;
            }

            if (string.IsNullOrWhiteSpace(entity.Status))
            {
                entity.Status = OrderStatus.PENDING.ToString();
            }

            try
            {
                await _orderRepository.UpdateAsync(entity);
                var updated = await GetById(entity.OrderId);
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order updated successfully.",
                    Data = updated
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Update order failed.",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<bool>> Delete(int id)
        {
            var entity = await _orderRepository.GetByIdAsync(id);

            if (entity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = false
                };
            }

            var hasRelatedData = await _orderRepository.HasRelatedDataAsync(id);
            if (hasRelatedData)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Cannot delete order because it is referenced by payment/order detail records.",
                    Data = false
                };
            }

            try
            {
                await _orderRepository.RemoveAsync(entity);
                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Order deleted successfully.",
                    Data = true
                };
            }
            catch (DbUpdateException)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Delete order failed due to related data constraints.",
                    Data = false
                };
            }
        }

        public async Task<ApiResponse<int>> CreateOrderAsync(CreateOrderRequest request)
        {
            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                return new ApiResponse<int>
                {
                    Success = false,
                    Message = "Customer not found.",
                    Data = 0
                };
            }

            if (request.Items == null || !request.Items.Any())
            {
                return new ApiResponse<int>
                {
                    Success = false,
                    Message = "Items are required.",
                    Data = 0
                };
            }

            var vietnamNow = GetVietnamNow();
            var minimumPartyDate = vietnamNow.Date.AddDays(2);
            var firstPartyDate = ToVietnamTime(request.Items[0].StartTime).Date;

            if (firstPartyDate < minimumPartyDate)
            {
                return new ApiResponse<int>
                {
                    Success = false,
                    Message = "First party date must be at least 2 days from today (Vietnam time).",
                    Data = 0
                };
            }

            for (var i = 0; i < request.Items.Count; i++)
            {
                if (request.Items[i].EndTime <= request.Items[i].StartTime)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = $"Item {i + 1}: EndTime must be greater than StartTime.",
                        Data = 0
                    };
                }

                var partyDate = ToVietnamTime(request.Items[i].StartTime).Date;
                if (partyDate < minimumPartyDate)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = $"Item {i + 1}: party date must be at least 2 days from today (Vietnam time).",
                        Data = 0
                    };
                }

                var dayDiffFromFirstParty = Math.Abs((partyDate - firstPartyDate).TotalDays);
                if (dayDiffFromFirstParty > 1)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "All party dates must be within 1 day from the first party date.",
                        Data = 0
                    };
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    CustomerId = request.CustomerId,
                    Status = OrderStatus.PENDING.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    TotalPrice = 0,
                    DepositAmount = 0,
                    RemainingAmount = 0
                };

                await _orderRepository.CreateAsync(order);

                decimal orderTotal = 0;
                var orderDetailSchedules = new List<(int OrderDetailId, DateTime? StartTime, DateTime? EndTime)>();

                foreach (var itemRequest in request.Items)
                {
                if (itemRequest.MenuId <= 0)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "MenuId must be greater than 0.",
                        Data = 0
                    };
                }

                if (itemRequest.NumberOfGuests <= 0)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "NumberOfGuests must be greater than 0.",
                        Data = 0
                    };
                }

                if (itemRequest.EndTime <= itemRequest.StartTime)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = "EndTime must be greater than StartTime.",
                        Data = 0
                    };
                }

                var menu = await _menuRepository.GetByIdAsync(itemRequest.MenuId);
                if (menu == null)
                {
                    return new ApiResponse<int>
                    {
                        Success = false,
                        Message = $"Menu with Id {itemRequest.MenuId} not found.",
                        Data = 0
                    };
                }

                int? partyCategoryId = null;
                if (itemRequest.PartyCategoryId > 0)
                {
                    var partyCategory = await _partyCategoryRepository.GetByIdAsync(itemRequest.PartyCategoryId);
                    if (partyCategory == null)
                    {
                        return new ApiResponse<int>
                        {
                            Success = false,
                            Message = $"PartyCategory with Id {itemRequest.PartyCategoryId} not found.",
                            Data = 0
                        };
                    }
                    partyCategoryId = itemRequest.PartyCategoryId;
                }

                var menuDishes = await _menuDishRepository
                    .GetAllMenuDishFiltered(new MenuDish { MenuId = menu.MenuId })
                    .ToListAsync();
                var menuDishIdSet = menuDishes
                    .Where(md => md.DishId.HasValue)
                    .Select(md => md.DishId!.Value)
                    .ToHashSet();

                var menuPrice = menu.BasePrice ?? 0;
                decimal itemServiceTotal = 0;
                decimal itemCustomDishTotal = 0;
                var serviceItems = new List<ServiceItemSnapshotDto>();
                var customDishItems = new List<CustomDishItemSnapshotDto>();

                if (itemRequest.Services != null && itemRequest.Services.Any())
                {
                    foreach (var svc in itemRequest.Services)
                    {
                        if (svc.ServiceId <= 0 || svc.Quantity <= 0)
                            continue;
                        var service = await _serviceRepository.GetByIdAsync(svc.ServiceId);
                        if (service != null && service.BasePrice.HasValue)
                        {
                            itemServiceTotal += service.BasePrice.Value * svc.Quantity;
                            serviceItems.Add(new ServiceItemSnapshotDto
                            {
                                ServiceId = service.ServiceId,
                                ServiceName = service.ServiceName,
                                BasePrice = service.BasePrice,
                                Quantity = svc.Quantity,
                                Img = service.Img
                            });
                        }
                    }
                }

                if (itemRequest.CustomDishes != null && itemRequest.CustomDishes.Any())
                {
                    foreach (var customDish in itemRequest.CustomDishes)
                    {
                        if (customDish.DishId <= 0)
                            continue;

                        if (menuDishIdSet.Contains(customDish.DishId))
                        {
                            return new ApiResponse<int>
                            {
                                Success = false,
                                Message = $"DishId {customDish.DishId} already exists in menu {itemRequest.MenuId}. Please remove it from custom dishes.",
                                Data = 0
                            };
                        }

                        var dish = await _dishRepository.GetByIdAsync(customDish.DishId);
                        if (dish == null)
                            continue;

                        var computedTotal = (dish.Price ?? 0) * itemRequest.NumberOfGuests;
                        decimal? normalizedTotal = computedTotal > 0 ? computedTotal : (decimal?)null;
                        customDishItems.Add(new CustomDishItemSnapshotDto
                        {
                            DishId = customDish.DishId,
                            DishName = dish.DishName,
                            UnitPrice = dish.Price,
                            TotalAmount = normalizedTotal,
                            Img = dish.Img
                        });

                        if (normalizedTotal.HasValue)
                        {
                            itemCustomDishTotal += normalizedTotal.Value;
                        }
                    }
                }

                var itemTotal = (menuPrice * itemRequest.NumberOfGuests) + itemServiceTotal + itemCustomDishTotal;

                var dishSnapshots = menuDishes
                    .Where(md => md.Dish != null)
                    .Select(md => new DishSnapshotDto
                    {
                        DishId = md.Dish!.DishId,
                        DishName = md.Dish.DishName,
                        Price = md.Dish.Price
                    })
                    .ToList();

                object? imgUrlObj = null;
                if (!string.IsNullOrEmpty(menu.ImgUrl))
                {
                    try
                    {
                        imgUrlObj = JsonSerializer.Deserialize<object>(menu.ImgUrl);
                    }
                    catch
                    {
                        imgUrlObj = new[] { menu.ImgUrl };
                    }
                }
                imgUrlObj ??= Array.Empty<string>();

                var menuSnapshot = new MenuSnapshotDto
                {
                    MenuName = menu.MenuName,
                    BasePrice = menu.BasePrice,
                    ImgUrl = imgUrlObj,
                    Dishes = dishSnapshots,
                    CapturedAt = DateTime.UtcNow.ToString("o")
                };

                var serviceSnapshot = new ServiceSnapshotDto
                {
                    Services = serviceItems,
                    CapturedAt = DateTime.UtcNow.ToString("o")
                };

                var hasCustomDishes = customDishItems.Any();
                var customDishSnapshot = hasCustomDishes
                    ? new CustomDishSnapshotDto
                    {
                        CustomDishes = customDishItems,
                        CapturedAt = DateTime.UtcNow.ToString("o")
                    }
                    : null;

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    Address = itemRequest.Address ?? string.Empty,
                    NumberOfGuests = itemRequest.NumberOfGuests,
                    Status = OrderDetailStatus.PENDING.ToString(),
                    TotalPrice = itemTotal,
                    Type = hasCustomDishes ? OrderDetailType.CUSTOM_ORDER.ToString() : OrderDetailType.ORDER.ToString(),
                    StartTime = itemRequest.StartTime,
                    EndTime = itemRequest.EndTime,
                    MenuId = itemRequest.MenuId,
                    PartyCategoryId = partyCategoryId,
                    MenuSnapshot = JsonSerializer.Serialize(menuSnapshot),
                    ServiceSnapshot = JsonSerializer.Serialize(serviceSnapshot),
                    CustomDishSnapshot = hasCustomDishes ? JsonSerializer.Serialize(customDishSnapshot) : null
                };

                await _orderDetailRepository.CreateAsync(orderDetail);
                orderDetailSchedules.Add((orderDetail.OrderDetailId, orderDetail.StartTime, orderDetail.EndTime));

                if (itemRequest.Services != null && itemRequest.Services.Any())
                {
                    foreach (var svc in itemRequest.Services)
                    {
                        if (svc.ServiceId <= 0 || svc.Quantity <= 0)
                            continue;
                        var service = await _serviceRepository.GetByIdAsync(svc.ServiceId);
                        if (service == null)
                            continue;

                        var orderService = new OrderService
                        {
                            OrderDetailId = orderDetail.OrderDetailId,
                            ServiceId = svc.ServiceId,
                            Quantity = svc.Quantity,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _orderServiceRepository.CreateAsync(orderService);
                    }
                }

                if (hasCustomDishes)
                {
                    foreach (var customDishItem in customDishItems)
                    {
                        var orderDetailCustom = new OrderDetailCustom
                        {
                            OrderDetailId = orderDetail.OrderDetailId,
                            DishId = customDishItem.DishId,
                            TotalAmount = customDishItem.TotalAmount
                        };

                        await _orderDetailCustomRepository.CreateAsync(orderDetailCustom);
                    }
                }

                    orderTotal += itemTotal;
                }

                order.TotalPrice = orderTotal;
                await _orderRepository.UpdateAsync(order);
                await transaction.CommitAsync();

                foreach (var schedule in orderDetailSchedules)
                {
                    await _orderStatusSchedulerService.ScheduleOrderDetailStatusTransitionsAsync(
                        schedule.OrderDetailId,
                        schedule.StartTime,
                        schedule.EndTime);
                }

                await _orderStatusSchedulerService.ScheduleOrderDepositTimeoutAsync(order.OrderId, order.CreatedAt);

                return new ApiResponse<int>
                {
                    Success = true,
                    Message = "Order created successfully.",
                    Data = order.OrderId
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                return new ApiResponse<int>
                {
                    Success = false,
                    Message = "Create order failed.",
                    Data = 0
                };
            }
        }

        public async Task<ApiResponse<OrderResponse>> UpdateCustomerOrderAsync(int orderId, UpdateCustomerOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Items are required.",
                    Data = null
                };
            }

            var order = await _dbContext.Orders
                .Include(x => x.Payments)
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            if (!string.Equals(order.Status, OrderStatus.PENDING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Only PENDING orders can be edited by customer.",
                    Data = null
                };
            }

            var hasPaidPayment = order.Payments?.Any(p =>
                string.Equals(p.PaymentStatus, PaymentStatus.PAID.ToString(), StringComparison.OrdinalIgnoreCase)) ?? false;

            if (hasPaidPayment)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order has been paid and cannot be edited.",
                    Data = null
                };
            }

            var hasAssignedStaffGroup = order.OrderDetails?.Any(x => x.StaffGroupId.HasValue) ?? false;
            if (hasAssignedStaffGroup)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order has already been assigned and cannot be edited.",
                    Data = null
                };
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    _dbContext.OrderDetails.RemoveRange(order.OrderDetails);
                    await _dbContext.SaveChangesAsync();
                }

                decimal orderTotal = 0;

                foreach (var itemRequest in request.Items)
                {
                    if (itemRequest.MenuId <= 0)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<OrderResponse>
                        {
                            Success = false,
                            Message = "MenuId must be greater than 0.",
                            Data = null
                        };
                    }

                    if (itemRequest.NumberOfGuests <= 0)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<OrderResponse>
                        {
                            Success = false,
                            Message = "NumberOfGuests must be greater than 0.",
                            Data = null
                        };
                    }

                    if (itemRequest.EndTime <= itemRequest.StartTime)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<OrderResponse>
                        {
                            Success = false,
                            Message = "EndTime must be greater than StartTime.",
                            Data = null
                        };
                    }

                    var menu = await _menuRepository.GetByIdAsync(itemRequest.MenuId);
                    if (menu == null)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<OrderResponse>
                        {
                            Success = false,
                            Message = $"Menu with Id {itemRequest.MenuId} not found.",
                            Data = null
                        };
                    }

                    int? partyCategoryId = null;
                    if (itemRequest.PartyCategoryId > 0)
                    {
                        var partyCategory = await _partyCategoryRepository.GetByIdAsync(itemRequest.PartyCategoryId);
                        if (partyCategory == null)
                        {
                            await transaction.RollbackAsync();
                            return new ApiResponse<OrderResponse>
                            {
                                Success = false,
                                Message = $"PartyCategory with Id {itemRequest.PartyCategoryId} not found.",
                                Data = null
                            };
                        }
                        partyCategoryId = itemRequest.PartyCategoryId;
                    }

                    var menuPrice = menu.BasePrice ?? 0;
                    decimal itemServiceTotal = 0;
                    var serviceItems = new List<ServiceItemSnapshotDto>();

                    if (itemRequest.Services != null && itemRequest.Services.Any())
                    {
                        foreach (var svc in itemRequest.Services)
                        {
                            if (svc.ServiceId <= 0 || svc.Quantity <= 0)
                            {
                                continue;
                            }

                            var service = await _serviceRepository.GetByIdAsync(svc.ServiceId);
                            if (service != null && service.BasePrice.HasValue)
                            {
                                itemServiceTotal += service.BasePrice.Value * svc.Quantity;
                                serviceItems.Add(new ServiceItemSnapshotDto
                                {
                                    ServiceId = service.ServiceId,
                                    ServiceName = service.ServiceName,
                                    BasePrice = service.BasePrice,
                                    Quantity = svc.Quantity,
                                    Img = service.Img
                                });
                            }
                        }
                    }

                    var itemTotal = (menuPrice * itemRequest.NumberOfGuests) + itemServiceTotal;

                    var menuDishes = await _menuDishRepository
                        .GetAllMenuDishFiltered(new MenuDish { MenuId = menu.MenuId })
                        .ToListAsync();
                    var dishSnapshots = menuDishes
                        .Where(md => md.Dish != null)
                        .Select(md => new DishSnapshotDto
                        {
                            DishId = md.Dish!.DishId,
                            DishName = md.Dish.DishName,
                            Price = md.Dish.Price
                        })
                        .ToList();

                    object? imgUrlObj = null;
                    if (!string.IsNullOrEmpty(menu.ImgUrl))
                    {
                        try
                        {
                            imgUrlObj = JsonSerializer.Deserialize<object>(menu.ImgUrl);
                        }
                        catch
                        {
                            imgUrlObj = new[] { menu.ImgUrl };
                        }
                    }
                    imgUrlObj ??= Array.Empty<string>();

                    var menuSnapshot = new MenuSnapshotDto
                    {
                        MenuName = menu.MenuName,
                        BasePrice = menu.BasePrice,
                        ImgUrl = imgUrlObj,
                        Dishes = dishSnapshots,
                        CapturedAt = DateTime.UtcNow.ToString("o")
                    };

                    var serviceSnapshot = new ServiceSnapshotDto
                    {
                        Services = serviceItems,
                        CapturedAt = DateTime.UtcNow.ToString("o")
                    };

                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        Address = itemRequest.Address ?? string.Empty,
                        NumberOfGuests = itemRequest.NumberOfGuests,
                        Status = OrderDetailStatus.PENDING.ToString(),
                        TotalPrice = itemTotal,
                        Type = OrderDetailType.ORDER.ToString(),
                        StartTime = itemRequest.StartTime,
                        EndTime = itemRequest.EndTime,
                        MenuId = itemRequest.MenuId,
                        PartyCategoryId = partyCategoryId,
                        NoteOrderDetail = string.IsNullOrWhiteSpace(itemRequest.NoteOrderDetail) ? null : itemRequest.NoteOrderDetail.Trim(),
                        MenuSnapshot = JsonSerializer.Serialize(menuSnapshot),
                        ServiceSnapshot = JsonSerializer.Serialize(serviceSnapshot)
                    };

                    await _orderDetailRepository.CreateAsync(orderDetail);
                    await _orderStatusSchedulerService.ScheduleOrderDetailStatusTransitionsAsync(orderDetail.OrderDetailId, orderDetail.StartTime, orderDetail.EndTime);

                    if (itemRequest.Services != null && itemRequest.Services.Any())
                    {
                        foreach (var svc in itemRequest.Services)
                        {
                            if (svc.ServiceId <= 0 || svc.Quantity <= 0)
                            {
                                continue;
                            }

                            var service = await _serviceRepository.GetByIdAsync(svc.ServiceId);
                            if (service == null)
                            {
                                continue;
                            }

                            var orderService = new OrderService
                            {
                                OrderDetailId = orderDetail.OrderDetailId,
                                ServiceId = svc.ServiceId,
                                Quantity = svc.Quantity,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _orderServiceRepository.CreateAsync(orderService);
                        }
                    }

                    orderTotal += itemTotal;
                }

                order.TotalPrice = orderTotal;
                order.DepositAmount = 0;
                order.RemainingAmount = orderTotal;
                order.Status = OrderStatus.PENDING.ToString();

                await _orderRepository.UpdateAsync(order);

                await transaction.CommitAsync();

                var updated = await GetById(orderId);
                return new ApiResponse<OrderResponse>
                {
                    Success = true,
                    Message = "Order updated successfully.",
                    Data = updated
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Update order failed.",
                    Data = null
                };
            }
        }

        public async Task<PagedResponse<OrderResponse>> GetDepositedApprovedForAssignmentAsync(int page, int pageSize)
        {
            var query = _orderRepository.GetDepositedApprovedOrdersForAssignment();
            var totalCount = await query.CountAsync();

            var items = await query
                .ProjectToType<OrderResponse>()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            await AttachExtraChargeCostsAsync(items);

            return new PagedResponse<OrderResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<OrderResponse>> AssignOrderToStaffGroupAsync(int orderId, int staffGroupId)
        {
            if (staffGroupId <= 0)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "StaffGroupId must be greater than 0.",
                    Data = null
                };
            }

            var order = await _orderRepository.GetByIdWithRelationAsync(orderId);
            if (order == null)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            if (!string.Equals(order.Status, OrderStatus.APPROVED.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Only APPROVED orders can be assigned.",
                    Data = null
                };
            }

            var isDeposited = (order.DepositAmount ?? 0) > 0 ||
                              (order.Payments?.Any(p =>
                                  p.PaymentType == PaymentType.DEPOSIT.ToString() &&
                                  p.PaymentStatus == PaymentStatus.PAID.ToString()) ?? false);

            if (!isDeposited)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order has not been deposited yet.",
                    Data = null
                };
            }

            var staffGroup = await _staffGroupRepository
                .GetAllStaffGroupFiltered(new StaffGroup { StaffGroupId = staffGroupId })
                .FirstOrDefaultAsync(x => x.Status == StaffGroupStatus.ACTIVE.ToString());

            if (staffGroup == null)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Staff group not found or inactive.",
                    Data = null
                };
            }

            if (!staffGroup.LeaderId.HasValue)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Staff group must have a leader before assignment.",
                    Data = null
                };
            }

            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order has no order details to assign.",
                    Data = null
                };
            }

            var hasUnassignedDetails = order.OrderDetails.Any(x => !x.StaffGroupId.HasValue);
            if (!hasUnassignedDetails)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order is already assigned to a staff group.",
                    Data = null
                };
            }

            await _orderDetailRepository.AssignOrderToStaffGroupAsync(orderId, staffGroup.StaffGroupId, OrderDetailStatus.PREPARING.ToString());

            order.Status = OrderStatus.PREPARING.ToString();
            await _orderRepository.UpdateAsync(order);

            if (staffGroup.LeaderId.HasValue)
            {
                await _notificationService.SendToUserAsync(
                    staffGroup.LeaderId.Value,
                    "Bạn được giao tiệc mới",
                    $"Đơn tiệc #{order.OrderId} đã được giao cho nhóm của bạn.",
                    NotificationType.Order,
                    new Dictionary<string, string>
                    {
                        ["orderId"] = order.OrderId.ToString(),
                        ["staffGroupId"] = staffGroup.StaffGroupId.ToString()
                    });
            }

            var updated = await GetById(orderId);
            return new ApiResponse<OrderResponse>
            {
                Success = true,
                Message = "Order assigned to staff group successfully.",
                Data = updated
            };
        }

        public async Task<ApiResponse<OrderResponse>> ReviewOrderAsync(int orderId, int status)
        {
            if (!System.Enum.IsDefined(typeof(OrderStatus), status))
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Invalid status value.",
                    Data = null
                };
            }

            var targetStatus = (OrderStatus)status;
            if (targetStatus != OrderStatus.APPROVED && targetStatus != OrderStatus.REJECTED)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Only APPROVED(2) or REJECTED(3) are allowed.",
                    Data = null
                };
            }

            var order = await _orderRepository.GetByIdWithRelationAsync(orderId);
            if (order == null)
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Order not found.",
                    Data = null
                };
            }

            if (!string.Equals(order.Status, OrderStatus.PENDING.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<OrderResponse>
                {
                    Success = false,
                    Message = "Only PENDING orders can be reviewed.",
                    Data = null
                };
            }

            order.Status = targetStatus.ToString();

            if (targetStatus == OrderStatus.REJECTED && order.OrderDetails != null && order.OrderDetails.Any())
            {
                foreach (var detail in order.OrderDetails)
                {
                    detail.Status = OrderDetailStatus.REJECTED.ToString();
                }
            }

            await _dbContext.SaveChangesAsync();
            await SendOrderReviewEmailAsync(order, targetStatus);

            var updated = await GetById(orderId);
            return new ApiResponse<OrderResponse>
            {
                Success = true,
                Message = $"Order has been {targetStatus} successfully.",
                Data = updated
            };
        }

        private async Task SendOrderReviewEmailAsync(Order order, OrderStatus targetStatus)
        {
            var toEmail = order.Customer?.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            var isApproved = targetStatus == OrderStatus.APPROVED;
            var statusLabel = isApproved ? "ĐÃ DUYỆT" : "TỪ CHỐI";
            var statusColor = isApproved ? "#16a34a" : "#dc2626";
            var statusBgColor = isApproved ? "#dcfce7" : "#fee2e2";
            var subject = targetStatus == OrderStatus.APPROVED
                ? $"[Bookfet] Đơn hàng #{order.OrderId} đã được duyệt"
                : $"[Bookfet] Đơn hàng #{order.OrderId} đã bị từ chối";

            var customerName = string.IsNullOrWhiteSpace(order.Customer?.FullName)
                ? "Quý khách"
                : order.Customer!.FullName;
            var rejectProfessionalBlock = isApproved
                ? string.Empty
                : @"
<div style=""margin:12px 0 14px 0;padding:12px;border-radius:10px;background:#fff1f2;border:1px solid #fecdd3;color:#7f1d1d;"">
  <p style=""margin:0 0 8px 0;font-weight:700;"">Rất tiếc, đơn hàng của bạn hiện chưa đáp ứng điều kiện để duyệt.</p>
  <p style=""margin:0;"">Đội ngũ Bookfet luôn sẵn sàng hỗ trợ bạn điều chỉnh thông tin tiệc phù hợp hơn để có thể đặt lại nhanh chóng.</p>
</div>";
            var rejectContactLine = isApproved
                ? string.Empty
                : @"<p style=""margin:14px 0 0 0;"">Nếu bạn cần hỗ trợ hoặc muốn trao đổi thêm, vui lòng liên hệ với chúng tôi qua kênh chat hoặc hotline của Bookfet. Chúng tôi sẽ ưu tiên phản hồi sớm nhất để bạn không bỏ lỡ kế hoạch sự kiện.</p>";
            var approvedPreparationBlock = string.Empty;

            if (isApproved)
            {
                var detailCards = (order.OrderDetails ?? new List<OrderDetail>())
                    .OrderBy(x => x.StartTime)
                    .Select(detail =>
                    {
                        var startTimeText = detail.StartTime.HasValue
                            ? ToVietnamTime(detail.StartTime.Value).ToString("dd/MM/yyyy HH:mm")
                            : "Chưa xác định";
                        var menuName = string.IsNullOrWhiteSpace(detail.Menu?.MenuName) ? "Tiệc đơn giản" : detail.Menu!.MenuName;
                        var menuImageUrl = GetFirstImageUrl(detail.Menu?.ImgUrl);
                        var imageHtml = string.IsNullOrWhiteSpace(menuImageUrl)
                            ? @"<div style=""height:120px;border-radius:10px;background:#e2e8f0;color:#334155;display:flex;align-items:center;justify-content:center;font-weight:600;"">Tiệc đơn giản</div>"
                            : $@"<img src=""{menuImageUrl}"" alt=""{menuName}"" style=""width:100%;height:120px;object-fit:cover;border-radius:10px;display:block;"" />";

                        return $@"
<div style=""border:1px solid #e2e8f0;border-radius:10px;padding:12px;margin-bottom:10px;background:#f8fafc;"">
  {imageHtml}
  <p style=""margin:10px 0 4px 0;font-weight:700;"">{menuName}</p>
  <p style=""margin:0;color:#334155;"">Mã tiệc: <strong>#{detail.OrderDetailId}</strong></p>
  <p style=""margin:4px 0 0 0;color:#334155;"">Thời gian bắt đầu: <strong>{startTimeText}</strong></p>
</div>";
                    })
                    .ToList();

                if (detailCards.Count > 0)
                {
                    approvedPreparationBlock = $@"
<div style=""margin:14px 0;"">
  <p style=""margin:0 0 8px 0;font-weight:700;"">Chúng tôi sẽ chuẩn bị các tiệc sau cho bạn:</p>
  {string.Join(string.Empty, detailCards)}
</div>";
                }
            }

            var htmlBody = $@"
<div style=""font-family:Arial,sans-serif;max-width:560px;margin:0 auto;padding:16px;background:#f8fafc;color:#0f172a;"">
  <div style=""background:#ffffff;border-radius:12px;padding:20px;border:1px solid #e2e8f0;"">
    <h2 style=""margin:0 0 12px 0;"">Thông báo trạng thái đơn hàng</h2>
    <p style=""margin:0 0 10px 0;"">Xin chào <strong>{customerName}</strong>,</p>
    <p style=""margin:0 0 12px 0;"">Đơn hàng <strong>#{order.OrderId}</strong> của bạn đã được hệ thống xử lý.</p>
    <p style=""margin:0 0 14px 0;"">
      Trạng thái:
      <span style=""display:inline-block;padding:4px 10px;border-radius:999px;background:{statusBgColor};color:{statusColor};font-weight:700;"">
        {statusLabel}
      </span>
    </p>
    {approvedPreparationBlock}
    {rejectProfessionalBlock}
    {rejectContactLine}
    <p style=""margin:0;"">Cảm ơn bạn đã sử dụng dịch vụ của Bookfet.</p>
  </div>
</div>";

            var approvedPlainTextLines = isApproved
                ? string.Join(
                    "; ",
                    (order.OrderDetails ?? new List<OrderDetail>())
                        .OrderBy(x => x.StartTime)
                        .Select(detail =>
                        {
                            var startTimeText = detail.StartTime.HasValue
                                ? ToVietnamTime(detail.StartTime.Value).ToString("dd/MM/yyyy HH:mm")
                                : "Chua xac dinh";
                            return $"tiec #{detail.OrderDetailId} bat dau luc {startTimeText}";
                        }))
                : string.Empty;

            var plainText = isApproved
                ? $"Xin chao {customerName}. Don hang #{order.OrderId} da duoc cap nhat trang thai: {statusLabel}. Chung toi se chuan bi cac tiec: {approvedPlainTextLines}."
                : $"Xin chao {customerName}. Don hang #{order.OrderId} da duoc cap nhat trang thai: {statusLabel}. Rat tiec don hang hien chua dap ung dieu kien de duyet. Neu ban can ho tro, vui long lien he chung toi qua kenh chat hoac hotline cua Bookfet de duoc uu tien ho tro som nhat.";

            try
            {
                await _emailService.SendAsync(toEmail, subject, htmlBody, plainText);
            }
            catch
            {
                // Email send failure should not break order review result.
            }
        }

        private static DateTime GetVietnamNow()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetVietnamTimeZone());
        }

        private static DateTime ToVietnamTime(DateTime input)
        {
            var utc = input.Kind switch
            {
                DateTimeKind.Utc => input,
                DateTimeKind.Local => input.ToUniversalTime(),
                _ => DateTime.SpecifyKind(input, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utc, GetVietnamTimeZone());
        }

        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
        }

        private static string? GetFirstImageUrl(string? rawImgUrl)
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
                    return images?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                }

                if (trimmed.StartsWith("\""))
                {
                    var single = JsonSerializer.Deserialize<string>(trimmed);
                    return string.IsNullOrWhiteSpace(single) ? null : single;
                }
            }
            catch
            {
                // Fallback below for malformed image JSON.
            }

            return trimmed;
        }

        private async Task AttachExtraChargeCostsAsync(List<OrderResponse> orders)
        {
            if (orders == null || orders.Count == 0)
            {
                return;
            }

            var detailIds = orders
                .SelectMany(x => x.OrderDetails ?? Enumerable.Empty<OrderDetailResponse>())
                .Select(x => x.OrderDetailId)
                .Distinct()
                .ToList();

            if (detailIds.Count == 0)
            {
                return;
            }

            var extraChargeCostByDetailId = await _dbContext.OrderDetailExtraCharges
                .Where(x => x.OrderDetailId.HasValue && detailIds.Contains(x.OrderDetailId.Value))
                .GroupBy(x => x.OrderDetailId!.Value)
                .Select(g => new
                {
                    OrderDetailId = g.Key,
                    ExtraChargeCost = g.Sum(x => x.TotalAmount)
                })
                .ToDictionaryAsync(x => x.OrderDetailId, x => x.ExtraChargeCost);

            foreach (var order in orders)
            {
                if (order.OrderDetails == null || order.OrderDetails.Count == 0)
                {
                    continue;
                }

                foreach (var detail in order.OrderDetails)
                {
                    detail.ExtraChargeCost = extraChargeCostByDetailId.TryGetValue(detail.OrderDetailId, out var cost)
                        ? cost
                        : null;
                }
            }
        }
    }
}