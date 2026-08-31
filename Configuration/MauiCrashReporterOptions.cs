namespace AkengMauiCrashReporter.Configuration
{
    public sealed class MauiCrashReporterOptions
    {
        public int MaxStoredReports { get; set; } = 20;

        public long MaxStorageSizeBytes { get; set; } = 10 * 1024 * 1024;

        public bool PreserveFatalReports { get; set; } = true;

        public bool CaptureUnhandledExceptions { get; set; } = true;

        public bool CaptureUnobservedTaskExceptions { get; set; } = true;

        public bool IncludeDeviceInfo { get; set; } = true;

        public bool IncludeAppInfo { get; set; } = true;

        public bool IncludeNavigationInfo { get; set; } = true;

        public bool EnableDiagnostics { get; set; } = true;

        public bool EnableBreadcrumbs { get; set; } = true;

        public int MaxBreadcrumbs { get; set; } = 50;

        public int MaxBreadcrumbMessageLength { get; set; } = 1024;

        public int MaxContextEntries { get; set; } = 20;

        public int MaxContextKeyLength { get; set; } = 128;

        public int MaxContextValueLength { get; set; } = 2048;

        public int MaxExceptionDepth { get; set; } = 8;

        public int MaxStackTraceLength { get; set; } = 32_000;

        public int CrashLoopThreshold { get; set; } = 3;

        public TimeSpan CrashLoopWindow { get; set; } = TimeSpan.FromMinutes(5);

        public int MaxExceptionMessageLength { get; set; } = 4096;

        public int MaxAggregateExceptions { get; set; } = 10;

        public TimeSpan StartupCrashThreshold { get; set; } = TimeSpan.FromSeconds(10);

        public bool? TrimmingEnabled { get; set; }

        public bool? AotEnabled { get; set; }

        public bool CaptureLifecycleEvents { get; set; } = true;

        public bool ShowCrashReportPopupOnNextStartup { get; set; } = true;

        public bool AllowCopyCrashReport { get; set; } = true;

        public bool AllowShareCrashReport { get; set; } = true;

        internal void Validate()
        {
            if (MaxStoredReports < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxStoredReports));

            if (MaxBreadcrumbs < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxBreadcrumbs));

            if (MaxBreadcrumbMessageLength < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxBreadcrumbMessageLength));

            if (MaxContextEntries < 0)
                throw new ArgumentOutOfRangeException(nameof(MaxContextEntries));

            if (MaxContextKeyLength < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxContextKeyLength));

            if (MaxContextValueLength < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxContextValueLength));

            if (MaxExceptionDepth < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxExceptionDepth));

            if (MaxStackTraceLength < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxStackTraceLength));

            if (CrashLoopThreshold < 2)
                throw new ArgumentOutOfRangeException(nameof(CrashLoopThreshold));

            if (CrashLoopWindow <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(CrashLoopWindow));

            if (MaxExceptionMessageLength < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxExceptionMessageLength));

            if (MaxAggregateExceptions < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxAggregateExceptions));

            if (StartupCrashThreshold <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(StartupCrashThreshold));
        }
    }
}
