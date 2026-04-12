using System.Threading;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Enum;
using BookfetSystem.Services.Models.Request;
using BookfetSystem.Services.Models.Response;
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
    }

    private static int? ParseNullableInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }
}

