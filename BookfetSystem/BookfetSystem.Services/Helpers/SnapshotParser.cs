using System.Text.Json;
using BookfetSystem.Services.Models;

namespace BookfetSystem.Services.Helpers
{
    /// <summary>
    /// Helper to parse JSONB/JSON string from DB into snapshot DTOs.
    /// </summary>
    public static class SnapshotParser
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Parses a raw JSON string to <see cref="MenuSnapshotDto"/>.
        /// Returns null if input is null/empty or parse fails.
        /// </summary>
        public static MenuSnapshotDto? TryParseMenuSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<MenuSnapshotDto>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a raw JSON string to <see cref="ServiceSnapshotDto"/>.
        /// Returns null if input is null/empty or parse fails.
        /// </summary>
        public static ServiceSnapshotDto? TryParseServiceSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ServiceSnapshotDto>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a raw JSON string to <see cref="CustomDishSnapshotDto"/>.
        /// Returns null if input is null/empty or parse fails.
        /// </summary>
        public static CustomDishSnapshotDto? TryParseCustomDishSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<CustomDishSnapshotDto>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a raw JSON string to <see cref="GuestDiscountSnapshotDto"/>.
        /// Returns null if input is null/empty or parse fails.
        /// </summary>
        public static GuestDiscountSnapshotDto? TryParseGuestDiscountSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<GuestDiscountSnapshotDto>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a raw JSON string to <see cref="ExtraChargeSnapshotDto"/>.
        /// Returns null if input is null/empty or parse fails.
        /// </summary>
        public static ExtraChargeSnapshotDto? TryParseExtraChargeSnapshot(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<ExtraChargeSnapshotDto>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a raw JSON string to object (array, object, etc.).
        /// Returns null if input is null/empty or parse fails.
        /// Use for JSONB fields like ImgUrl.
        /// </summary>
        public static object? TryParseJsonToObject(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<object>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a raw JSON string to list of string.
        /// Returns null if input is null/empty or parse fails.
        /// Use for JSONB fields storing array of image URLs.
        /// </summary>
        public static List<string>? TryParseJsonToStringList(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }
}
