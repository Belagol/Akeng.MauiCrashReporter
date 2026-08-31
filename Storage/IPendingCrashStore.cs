using AkengMauiCrashReporter.Models;

namespace AkengMauiCrashReporter.Storage
{
    internal interface IPendingCrashStore
    {
        bool HasPendingCrash { get; }

        Task<IReadOnlyList<EmergencyCrashReport>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<EmergencyCrashReport?> GetLatestAsync(CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid crashId, CancellationToken cancellationToken = default);

        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
