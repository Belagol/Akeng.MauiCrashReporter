namespace AkengMauiCrashReporter.Models
{
    public sealed class CrashSessionInfo
    {
        public Guid SessionId { get; init; }

        public DateTimeOffset StartedAtUtc { get; init; }

        public TimeSpan? Duration { get; init; }
    }
}
