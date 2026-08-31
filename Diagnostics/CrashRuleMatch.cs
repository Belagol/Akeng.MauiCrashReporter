namespace AkengMauiCrashReporter.Diagnostics
{
    internal sealed class CrashRuleMatch
    {
        public int Score { get; init; }

        public IReadOnlyList<string> Evidence { get; init; } = [];
    }
}
