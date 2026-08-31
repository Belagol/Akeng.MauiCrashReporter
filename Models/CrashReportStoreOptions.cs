namespace AkengMauiCrashReporter.Models
{
    public sealed class CrashReportStoreOptions
    {
        public int MaxStoredReports { get; set; } = 20;

        public long MaxStorageSizeBytes { get; set; } = 10 * 1024 * 1024;

        public bool PreserveFatalReports { get; set; } = true;
    }
}
