namespace AkengMauiCrashReporter.Formatting
{
    public sealed class CrashReportFormattingOptions
    {
        public bool IncludeStackTrace { get; set; } = true;

        public bool IncludeBreadcrumbs { get; set; } = true;

        public bool IncludeContext { get; set; } = true;

        public bool IncludeEnvironment { get; set; } = true;

        public bool IncludeDiagnostic { get; set; } = true;

        public bool IncludeSession { get; set; } = true;

        public bool IncludeFingerprint { get; set; } = true;

        public int MaxBreadcrumbs { get; set; } = 50;
    }
}
