using AkengMauiCrashReporter.Models;

namespace AkengMauiCrashReporter.Storage
{
    public interface ICrashReportStore
    {
        Task SaveAsync(CrashReport report, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StoredCrashReport>> GetPendingAsync(CancellationToken cancellationToken = default);

        Task<StoredCrashReport?> GetLatestAsync(CancellationToken cancellationToken = default);

        Task<StoredCrashReport?> GetAsync(Guid id, CancellationToken cancellationToken = default);

        Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task ClearAsync(CancellationToken cancellationToken = default);

        Task<int> CountAsync(CancellationToken cancellationToken = default);

        Task<StoredCrashReport?> GetLatestUnpresentedFatalReportAsync(CancellationToken cancellationToken = default);
        Task MarkAsPresentedAsync(Guid reportId, CancellationToken cancellationToken = default);
    }
}
