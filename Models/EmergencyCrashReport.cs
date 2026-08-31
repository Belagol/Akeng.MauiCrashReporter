using AkengMauiCrashReporter.Models.Enums;

namespace AkengMauiCrashReporter.Models
{
    internal sealed class EmergencyCrashReport
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid SessionId { get; init; }

        public DateTimeOffset SessionStartedAtUtc { get; init; }

        public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

        public CrashOrigin Origin { get; init; }

        public string ExceptionType { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;

        public string? ExceptionText { get; init; }

        public string? AppVersion { get; init; }
    }
}
