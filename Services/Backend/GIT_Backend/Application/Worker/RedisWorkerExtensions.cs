namespace GIT_Backend.Application.Worker
{
    using System.Globalization;

    public static class RedisWorkerExtensions
    {
        public static string GetRequiredField(this Dictionary<string, string> values, string key)
        {
            if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Required Redis stream field is missing. field={key}");

            return value;
        }

        public static string? GetOptionalField(this Dictionary<string, string> values, string key)
        {
            return values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : null;
        }

        public static DateTimeOffset? GetOptionalDateTimeOffsetField(this Dictionary<string, string> values, string key)
        {
            var value = values.GetOptionalField(key);
            if (value is null)
                return null;

            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}
