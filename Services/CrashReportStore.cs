//using AkengMauiCrashReporter.Configuration;
//using AkengMauiCrashReporter.Models;
//using AkengMauiCrashReporter.Storage;
//using System.Text.Json;

//namespace AkengMauiCrashReporter.Services
//{
//    internal sealed class CrashReportStore : ICrashReportStore
//    {
//        private readonly MauiCrashReporterOptions _options;

//        public CrashReportStore(MauiCrashReporterOptions options)
//        {
//            _options = options;            
//        }

//        public async Task SaveAsync(CrashReport report, CancellationToken cancellationToken = default)
//        {
//            ArgumentNullException.ThrowIfNull(report);

//            cancellationToken.ThrowIfCancellationRequested();

//            try
//            {
//                Directory.CreateDirectory(CrashStoragePaths.ReportsDirectory);

//                var fileName = $"report_{report.TimestampUtc:yyyyMMdd_HHmmss_fff}_{report.Id:N}.json";

//                var finalPath = Path.Combine(CrashStoragePaths.ReportsDirectory, fileName);

//                var tempPath = finalPath + ".tmp";

//                try
//                {
//                    var json = JsonSerializer.Serialize(report);

//                    await File.WriteAllTextAsync(tempPath, json, cancellationToken);

//                    File.Move(tempPath, finalPath, overwrite: true);
//                }
//                finally
//                {
//                    TryDelete(tempPath);
//                }

//                CleanupOldReports();
//            }
//            catch (OperationCanceledException)
//            {
//                throw;
//            }
//            catch
//            {
//                // Storage errors must not break the host app.
//            }
//        }

//        public async Task<CrashReport?> GetLatestAsync(CancellationToken cancellationToken = default)
//        {
//            var reports = await GetAllAsync(cancellationToken);

//            return reports.OrderByDescending(x => x.TimestampUtc).FirstOrDefault();
//        }

//        public async Task<IReadOnlyList<CrashReport>> GetAllAsync(CancellationToken cancellationToken = default)
//        {
//            if (!Directory.Exists(CrashStoragePaths.ReportsDirectory))
//            {
//                return [];
//            }

//            string[] files;

//            try
//            {
//                files = Directory.EnumerateFiles(CrashStoragePaths.ReportsDirectory, "*.json", SearchOption.TopDirectoryOnly).ToArray();
//            }
//            catch
//            {
//                return [];
//            }

//            var reports = new List<CrashReport>();

//            foreach (var file in files)
//            {
//                cancellationToken.ThrowIfCancellationRequested();

//                var report = await TryReadAsync(file, cancellationToken);

//                if (report is not null)
//                {
//                    reports.Add(report);
//                }
//            }

//            return reports.OrderByDescending(x => x.TimestampUtc).ToArray();
//        }

//        public Task DeleteAsync(Guid reportId, CancellationToken cancellationToken = default)
//        {
//            cancellationToken.ThrowIfCancellationRequested();

//            if (!Directory.Exists(CrashStoragePaths.ReportsDirectory))
//            {
//                return Task.CompletedTask;
//            }

//            try
//            {
//                foreach (var file in Directory.EnumerateFiles(CrashStoragePaths.ReportsDirectory, "*.json", SearchOption.TopDirectoryOnly))
//                {
//                    cancellationToken.ThrowIfCancellationRequested();

//                    if (!Path.GetFileName(file).Contains(reportId.ToString("N"), StringComparison.OrdinalIgnoreCase))
//                    {
//                        continue;
//                    }

//                    TryDelete(file);
//                }
//            }
//            catch
//            {
//            }

//            return Task.CompletedTask;
//        }

//        public Task ClearAsync(CancellationToken cancellationToken = default)
//        {
//            cancellationToken.ThrowIfCancellationRequested();

//            if (!Directory.Exists(CrashStoragePaths.ReportsDirectory))
//            {
//                return Task.CompletedTask;
//            }

//            try
//            {
//                foreach (var file in Directory.EnumerateFiles(CrashStoragePaths.ReportsDirectory, "*.json", SearchOption.TopDirectoryOnly))
//                {
//                    cancellationToken.ThrowIfCancellationRequested();

//                    TryDelete(file);
//                }
//            }
//            catch
//            {
//            }

//            return Task.CompletedTask;
//        }

//        private static async Task<CrashReport?> TryReadAsync(string file, CancellationToken cancellationToken)
//        {
//            try
//            {
//                var json = await File.ReadAllTextAsync(file, cancellationToken);

//                if (string.IsNullOrWhiteSpace(json))
//                    return null;

//                return JsonSerializer.Deserialize<CrashReport>(json);
//            }
//            catch (OperationCanceledException)
//            {
//                throw;
//            }
//            catch
//            {
//                MoveToCorrupted(file);

//                return null;
//            }
//        }

//        private void CleanupOldReports()
//        {
//            try
//            {
//                if (!Directory.Exists(CrashStoragePaths.ReportsDirectory))
//                {
//                    return;
//                }

//                var files = Directory.EnumerateFiles(CrashStoragePaths.ReportsDirectory, "*.json", SearchOption.TopDirectoryOnly)
//                        .OrderByDescending(File.GetCreationTimeUtc)
//                        .Skip(_options.MaxStoredReports)
//                        .ToArray();

//                foreach (var file in files)
//                {
//                    TryDelete(file);
//                }
//            }
//            catch
//            {
//            }
//        }

//        private static void MoveToCorrupted(string file)
//        {
//            try
//            {
//                Directory.CreateDirectory(CrashStoragePaths.CorruptedDirectory);

//                var destination = Path.Combine(CrashStoragePaths.CorruptedDirectory, Path.GetFileName(file));

//                File.Move(file, destination, overwrite: true);
//            }
//            catch
//            {
//                TryDelete(file);
//            }
//        }

//        private static void TryDelete(string file)
//        {
//            try
//            {
//                if (File.Exists(file))
//                {
//                    File.Delete(file);
//                }
//            }
//            catch
//            {
//            }
//        }
//    }
//}
