using System.Threading;
using BookfetSystem.Repositories.Entities;
using BookfetSystem.Services.Helpers;
using BookfetSystem.Services.Enum;
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
    }

    private static int? ParseNullableInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }
}

