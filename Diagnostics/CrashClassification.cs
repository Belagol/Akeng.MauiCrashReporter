namespace AkengMauiCrashReporter.Diagnostics
{
    public sealed class CrashClassification
    {
        public CrashCategory Category { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public string? LikelyCause { get; init; }

        public string? Recommendation { get; init; }

        public double Confidence { get; init; }

        public bool IsReleaseSensitive { get; init; }

        public IReadOnlyList<string> Evidence { get; init; } = [];
        public int Score { get; init; }
    }
}
