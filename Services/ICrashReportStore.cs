//using AkengMauiCrashReporter.Models;
//using AkengMauiCrashReporter.Storage;

//namespace AkengMauiCrashReporter.Services
//{
//    internal interface ICrashReportStore
//    {
//        Task SaveAsync(
//       CrashReport report,
//       CancellationToken cancellationToken = default);

//        Task<IReadOnlyList<StoredCrashReport>>
//            GetPendingAsync(
//                CancellationToken cancellationToken = default);

//        Task<StoredCrashReport?> GetLatestAsync(CancellationToken cancellationToken = default);

//        Task<StoredCrashReport?>
//            GetAsync(
//                Guid id,
//                CancellationToken cancellationToken = default);

//        Task MarkAsProcessedAsync(
//            Guid id,
//            CancellationToken cancellationToken = default);

//        Task DeleteAsync(
//            Guid id,
//            CancellationToken cancellationToken = default);

//        Task ClearAsync(
//            CancellationToken cancellationToken = default);

//        Task<int> CountAsync(
//            CancellationToken cancellationToken = default);

//        //Task SaveAsync(CrashReport report, CancellationToken cancellationToken = default);

//        //Task<CrashReport?> GetLatestAsync(CancellationToken cancellationToken = default);

//        Task<IReadOnlyList<CrashReport>> GetAllAsync(CancellationToken cancellationToken = default);

//        //Task DeleteAsync(Guid reportId, CancellationToken cancellationToken = default);

//        //Task ClearAsync(CancellationToken cancellationToken = default);
//    }
//}
