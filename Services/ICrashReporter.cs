using AkengMauiCrashReporter.Formatting;
using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Models.Enums;

namespace AkengMauiCrashReporter.Services
{
    public interface ICrashReporter
    {
        bool IsInitialized { get; }
        bool HasPendingCrash { get; }
        Guid SessionId { get; }
        //Task InitializeAsync(CancellationToken cancellationToken = default);
        Task<CrashReport?> CaptureExceptionAsync(Exception exception, CancellationToken cancellationToken = default);
        Task<CrashReport?> CaptureExceptionAsync(Exception exception, CrashOrigin origin, CrashSeverity severity = CrashSeverity.NonFatal, CancellationToken cancellationToken = default);
        void AddBreadcrumb(string message, string? category = null);
        void SetContext(string key, string value);
        void RemoveContext(string key);
        void ClearContext();
        void ClearBreadcrumbs();
        Task<IReadOnlyList<StoredCrashReport>> GetPendingReportsAsync(CancellationToken cancellationToken = default);
        Task<StoredCrashReport?> GetLatestReportAsync(CancellationToken cancellationToken = default);
        Task<StoredCrashReport?> GetReportAsync(Guid id, CancellationToken cancellationToken = default);
        Task MarkReportAsProcessedAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeleteReportAsync(Guid id, CancellationToken cancellationToken = default);
        Task ClearReportsAsync(CancellationToken cancellationToken = default);
        Task<int> GetReportCountAsync(CancellationToken cancellationToken = default);
        string FormatReport(CrashReport report, CrashReportFormat format = CrashReportFormat.Markdown);
    }
}
