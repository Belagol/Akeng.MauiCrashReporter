namespace AkengMauiCrashReporter.Models
{
    public sealed class CrashLoopInfo
    {
        public bool IsDetected { get; init; }

        public string? Fingerprint { get; init; }

        public int Occurrences { get; init; }

        public TimeSpan Window { get; init; }

        public bool IsStartupCrash { get; init; }

        public IReadOnlyList<Guid> SessionIds { get; init; } = [];
    }
}
