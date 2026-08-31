namespace AkengMauiCrashReporter.Diagnostics
{
    internal sealed class CrashClassificationResult
    {
        public CrashClassification? Primary { get; init; }

        public IReadOnlyList<CrashClassification> Secondary { get; init; } = [];

        public IReadOnlyList<CrashClassification> AllMatches { get; init; } = [];
    }
}
