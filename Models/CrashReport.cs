using AkengMauiCrashReporter.Diagnostics;
using AkengMauiCrashReporter.Models.Enums;

namespace AkengMauiCrashReporter.Models
{
    public sealed class CrashReport
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string SchemaVersion { get; init; } = "1.0";

        public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;

        public CrashSeverity Severity { get; init; }

        public CrashOrigin Origin { get; init; }

        public string Fingerprint { get; init; } = string.Empty;

        public CrashExceptionInfo Exception { get; init; } = new();

        public CrashApplicationInfo? Application { get; init; }

        public CrashDeviceInfo? Device { get; init; }

        public CrashNavigationInfo? Navigation { get; init; }

        public CrashSessionInfo? Session { get; init; }

        public IReadOnlyList<CrashBreadcrumb> Breadcrumbs { get; init; } = [];

        public IReadOnlyDictionary<string, string> Context { get; init; } = new Dictionary<string, string>();

        public DiagnosticResult? Diagnostic { get; init; }

        public CrashClassification? Classification { get; init; }
        public EnvironmentSnapshot? Environment { get; init; }
        public ReleaseDiagnostic? ReleaseDiagnostic { get; init; }
    }
}
