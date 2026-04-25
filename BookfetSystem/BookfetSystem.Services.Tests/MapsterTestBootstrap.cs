using System;
using System.Linq;
using System.Threading;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
using BookfetSystem.Services.Mappings;
using Mapster;

namespace BookfetSystem.Services.Tests;

internal static class MapsterTestBootstrap
{
    private static int _configured;

    public static void EnsureConfigured()
    {
        if (Interlocked.Exchange(ref _configured, 1) == 1)
        {
            return;
        }

        // IMPORTANT: Mapster locks configuration after first Adapt/ProjectToType.
        // Configure ALL needed mappings exactly once for the whole test run.
        TypeAdapterConfig.GlobalSettings.NewConfig<Menu, MenuResponse>()
            .Map(dest => dest.Status, src => ParseNullableInt(src.Status));

        TypeAdapterConfig.GlobalSettings.NewConfig<MenuCategory, MenuCategoryResponse>()
            .Map(dest => dest.Status, src => EnumHelper.TryParseToInt<MenuStatus>(src.Status));

        TypeAdapterConfig.GlobalSettings.NewConfig<PartyCategory, PartyCategoryResponse>()
            .Map(dest => dest.Status, src => EnumHelper.TryParseToInt<PartyCategoryStatus>(src.Status));

        TypeAdapterConfig.GlobalSettings.NewConfig<DishFilterRequest, Dish>()
            .IgnoreNullValues(true)
            .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

        TypeAdapterConfig.GlobalSettings.NewConfig<Dish, DishResponse>()
            .Map(dest => dest.Status, src => EnumHelper.TryParseToInt<DishStatus>(src.Status))
            .Map(dest => dest.DishCategoryName,
                src => src.DishCategory != null ? src.DishCategory.DishCategoryName : null);

        TypeAdapterConfig.GlobalSettings.NewConfig<DishCategoryFilterRequest, DishCategory>()
            .IgnoreNullValues(true);

        TypeAdapterConfig.GlobalSettings.NewConfig<DishCategory, DishCategoryResponse>();

        TypeAdapterConfig.GlobalSettings.NewConfig<IngredientFilterRequest, Ingredient>()
            .IgnoreNullValues(true);

        TypeAdapterConfig.GlobalSettings.NewConfig<Ingredient, IngredientResponse>();

        TypeAdapterConfig.GlobalSettings.NewConfig<ServiceFilterRequest, Service>()
            .IgnoreNullValues(true)
            .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

        TypeAdapterConfig.GlobalSettings.NewConfig<Service, ServiceResponse>()
            .Map(dest => dest.Status, src => EnumHelper.TryParseToInt<ServiceStatus>(src.Status));

        new MenuDishMapping().Register(TypeAdapterConfig.GlobalSettings);
        new DishDetailMapping().Register(TypeAdapterConfig.GlobalSettings);
        new PartyCategoryMenuMapping().Register(TypeAdapterConfig.GlobalSettings);

        TypeAdapterConfig.GlobalSettings.NewConfig<OrderDetail, OrderDetailResponse>()
            .Map(dest => dest.MenuName,
                src => src.Menu != null ? src.Menu.MenuName : null)
            .Map(dest => dest.PartyCategoryName,
                src => src.PartyCategory != null ? src.PartyCategory.PartyCategoryName : null)
            .Map(dest => dest.ServiceDurationMinutes,
                src => src.PartyCategory != null ? src.PartyCategory.ServiceDurationMinutes : null)
            .Map(dest => dest.MenuSnapshot,
                src => SnapshotParser.TryParseMenuSnapshot(src.MenuSnapshot))
            .Map(dest => dest.ServiceSnapshot,
                src => SnapshotParser.TryParseServiceSnapshot(src.ServiceSnapshot))
            .Map(dest => dest.CustomDishSnapshot,
                src => SnapshotParser.TryParseCustomDishSnapshot(src.CustomDishSnapshot))
            .Map(dest => dest.GuestDiscountSnapshot,
                src => SnapshotParser.TryParseGuestDiscountSnapshot(src.GuestDiscountSnapshot))
            .Map(dest => dest.ExtraChargeSnapshot,
                src => SnapshotParser.TryParseExtraChargeSnapshot(src.ExtraChargeSnapshot))
            .Map(dest => dest.Status,
                src => EnumHelper.TryParseToInt<OrderDetailStatus>(src.Status))
            .Map(dest => dest.Type,
                src => EnumHelper.TryParseToInt<OrderDetailType>(src.Type))
            .Map(dest => dest.ExtraChargeCost,
                src => src.OrderDetailExtraCharges != null && src.OrderDetailExtraCharges.Any()
                    ? src.OrderDetailExtraCharges.Sum(ec => ec.TotalAmount)
                    : null);

        TypeAdapterConfig.GlobalSettings.NewConfig<OrderFilterRequest, Order>()
            .IgnoreNullValues(true)
            .Map(dest => dest.Status, src => src.Status.HasValue ? src.Status.Value.ToString() : null);

        TypeAdapterConfig.GlobalSettings.NewConfig<Order, OrderResponse>()
            .Map(dest => dest.CustomerName,
                src => src.Customer != null ? src.Customer.FullName : null)
            .Map(dest => dest.Status,
                src => EnumHelper.TryParseToInt<OrderStatus>(src.Status))
            .Map(dest => dest.OrderDetails,
                src => src.OrderDetails != null
                    ? src.OrderDetails.Select(od => od.Adapt<OrderDetailResponse>()).ToList()
                    : new List<OrderDetailResponse>());

        new StaffGroupAssignmentOverviewMapping().Register(TypeAdapterConfig.GlobalSettings);
        new OrderDetailStaffTaskMapping().Register(TypeAdapterConfig.GlobalSettings);
        new NotificationMapping().Register(TypeAdapterConfig.GlobalSettings);
        new UserMapping().Register(TypeAdapterConfig.GlobalSettings);
        new ConversationMapping().Register(TypeAdapterConfig.GlobalSettings);
        new MessageMapping().Register(TypeAdapterConfig.GlobalSettings);
        new ExtraChargeCatalogMapping().Register(TypeAdapterConfig.GlobalSettings);
        new PostMapping().Register(TypeAdapterConfig.GlobalSettings);
        new StaffGroupMapping().Register(TypeAdapterConfig.GlobalSettings);
        new StaffGroupMemberMapping().Register(TypeAdapterConfig.GlobalSettings);
    }

    private static int? ParseNullableInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }
}

