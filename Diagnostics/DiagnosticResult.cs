namespace AkengMauiCrashReporter.Diagnostics
{
    public sealed class DiagnosticResult
    {
        public CrashClassification Classification { get; init; } = new();

        public string Summary { get; init; } = string.Empty;

        public string? LikelyCause { get; init; }

        public IReadOnlyList<DiagnosticAction> RecommendedActions { get; init; } = [];

        public IReadOnlyList<string> ReleaseChecks { get; init; } = [];

        public bool RequiresDeveloperAttention { get; init; }

        public bool IsLikelyReleaseSpecific => Classification.IsReleaseSensitive;
    }
}
