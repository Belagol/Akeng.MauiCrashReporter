using AkengMauiCrashReporter.Models.Enums;

namespace AkengMauiCrashReporter.Models
{
    public sealed class StoredCrashReport
    {
        public Guid Id { get; init; }

        public CrashReportStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; init; }

        public DateTime? ProcessedAtUtc { get; set; }
        public bool HasBeenPresented { get; set; }

        public CrashReport Report { get; init; } = null!;
    }
}
