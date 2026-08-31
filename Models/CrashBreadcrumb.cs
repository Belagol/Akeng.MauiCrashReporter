namespace AkengMauiCrashReporter.Models
{
    public sealed class CrashBreadcrumb
    {
        public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

        public string Message { get; init; } = string.Empty;

        public string? Category { get; init; }
        public IReadOnlyDictionary<string, string> Data { get; init; }
    }
}
