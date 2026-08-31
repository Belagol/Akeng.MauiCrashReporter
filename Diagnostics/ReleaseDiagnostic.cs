namespace AkengMauiCrashReporter.Diagnostics
{
    public sealed class ReleaseDiagnostic
    {
        public bool IsSuspected { get; init; }

        public double Confidence { get; init; }

        public string? Reason { get; init; }

        public IReadOnlyList<string> Signals { get; init; } = [];
    }
}
