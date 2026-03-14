namespace BookfetSystem.Services.Helpers
{
    /// <summary>
    /// Helper to convert enum between string and integer value.
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// Parses enum string (e.g. "PENDING") to integer value. Returns null if invalid.
        /// </summary>
        public static int? TryParseToInt<TEnum>(string? value) where TEnum : struct, System.Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return System.Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
                ? (int)(object)parsed
                : null;
        }
    }
}
