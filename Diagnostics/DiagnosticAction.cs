namespace AkengMauiCrashReporter.Diagnostics
{
    public sealed class DiagnosticAction
    {
        public int Priority { get; init; }

        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
    }
}
