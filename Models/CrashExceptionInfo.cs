namespace AkengMauiCrashReporter.Models
{
    public sealed class CrashExceptionInfo
    {
        public string Type { get; init; } = string.Empty;

        public string Message { get; init; } = string.Empty;

        public string? StackTrace { get; init; }

        public string? Source { get; init; }

        public int HResult { get; init; }

        public CrashExceptionInfo? InnerException { get; init; }

        public IReadOnlyList<CrashExceptionInfo> InnerExceptions { get; init; } = [];
    }
}
