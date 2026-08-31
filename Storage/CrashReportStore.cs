using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Models.Enums;
using System.Text.Json;

namespace AkengMauiCrashReporter.Storage
{
    internal sealed class CrashReportStore : ICrashReportStore
    {
        private readonly CrashReportStoreOptions _options;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = false
        };

        private string StorageDirectory => CrashStoragePaths.ReportsDirectory;

        public CrashReportStore(CrashReportStoreOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            _options = options;
            Directory.CreateDirectory(StorageDirectory);
        }

        public async Task<StoredCrashReport?> GetLatestUnpresentedFatalReportAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var reports = await ReadAllAsync(cancellationToken);

                return reports
                    .Where(x => !x.HasBeenPresented && x.Report.Severity == CrashSeverity.Fatal)
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .FirstOrDefault();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task MarkAsPresentedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var path = GetPath(id);

                if (!File.Exists(path))
                    return;

                var report = await TryReadAsync(path, cancellationToken);

                if (report is null)
                    return;

                report.HasBeenPresented = true;

                await WriteAsync(path, report, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveAsync(CrashReport report, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(report);

            await _lock.WaitAsync(cancellationToken);

            try
            {
                var stored = new StoredCrashReport
                {
                    Id = Guid.NewGuid(),
                    Status = CrashReportStatus.Pending,
                    CreatedAtUtc = DateTime.UtcNow,
                    HasBeenPresented = false,
                    Report = report
                };

                var path = GetPath(stored.Id);

                await WriteAsync(path, stored, cancellationToken);

                await CleanupAsync(cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IReadOnlyList<StoredCrashReport>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var reports = await ReadAllAsync(cancellationToken);

                return reports
                    .Where(x => x.Status == CrashReportStatus.Pending)
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .ToArray();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<StoredCrashReport?> GetLatestAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var reports = await ReadAllAsync(cancellationToken);

                return reports
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .FirstOrDefault();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<StoredCrashReport?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var path = GetPath(id);

                if (!File.Exists(path))
                    return null;

                return await TryReadAsync(path, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var path = GetPath(id);

                if (!File.Exists(path))
                    return;

                var report = await TryReadAsync(path, cancellationToken);

                if (report is null)
                    return;

                report.Status = CrashReportStatus.Processed;
                report.ProcessedAtUtc = DateTime.UtcNow;

                await WriteAsync(path, report, cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                DeleteInternal(id);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                if (!Directory.Exists(StorageDirectory))
                    return;

                foreach (var file in Directory.EnumerateFiles(StorageDirectory, "*.json", SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    TryDelete(file);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken);

            try
            {
                var reports = await ReadAllAsync(cancellationToken);

                return reports.Count;
            }
            finally
            {
                _lock.Release();
            }
        }

        private string GetPath(Guid id)
        {
            return Path.Combine(StorageDirectory, $"{id:N}.json");
        }

        private async Task<List<StoredCrashReport>> ReadAllAsync(CancellationToken cancellationToken)
        {
            var reports = new List<StoredCrashReport>();

            if (!Directory.Exists(StorageDirectory))
                return reports;

            foreach (var path in Directory.EnumerateFiles(StorageDirectory, "*.json", SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var report = await TryReadAsync(path, cancellationToken);

                if (report is not null)
                {
                    reports.Add(report);
                }
            }

            return reports;
        }

        private async Task<StoredCrashReport?> TryReadAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                var json = await File.ReadAllTextAsync(path, cancellationToken);

                if (string.IsNullOrWhiteSpace(json))
                {
                    MoveToCorrupted(path);
                    return null;
                }

                return JsonSerializer.Deserialize<StoredCrashReport>(json, _jsonOptions);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                MoveToCorrupted(path);
                return null;
            }
        }

        private async Task WriteAsync(string path, StoredCrashReport report, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(report, _jsonOptions);

            var temporaryPath = $"{path}.tmp";

            try
            {
                await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                File.Move(temporaryPath, path);
            }
            finally
            {
                TryDelete(temporaryPath);
            }
        }

        private async Task CleanupAsync(CancellationToken cancellationToken)
        {
            var reports = await ReadAllAsync(cancellationToken);

            var ordered = reports
                .OrderBy(x => x.CreatedAtUtc)
                .ToList();

            while (ordered.Count > _options.MaxStoredReports)
            {
                var candidate = ordered.FirstOrDefault(CanDelete);

                if (candidate is null)
                    break;

                DeleteInternal(candidate.Id);

                ordered.Remove(candidate);
            }

            await CleanupBySizeAsync(ordered, cancellationToken);
        }

        private async Task CleanupBySizeAsync(List<StoredCrashReport> reports, CancellationToken cancellationToken)
        {
            long totalSize = 0;

            foreach (var report in reports)
            {
                var path = GetPath(report.Id);

                if (File.Exists(path))
                {
                    totalSize += new FileInfo(path).Length;
                }
            }

            while (totalSize > _options.MaxStorageSizeBytes)
            {
                var candidate = reports
                    .Where(CanDelete)
                    .OrderBy(x => x.CreatedAtUtc)
                    .FirstOrDefault();

                if (candidate is null)
                    break;

                var path = GetPath(candidate.Id);

                var size = File.Exists(path) ? new FileInfo(path).Length : 0;

                DeleteInternal(candidate.Id);

                reports.Remove(candidate);

                totalSize -= size;

                await Task.Yield();

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private bool CanDelete(StoredCrashReport report)
        {
            if (!_options.PreserveFatalReports)
                return true;

            return report.Report.Severity != CrashSeverity.Fatal;
        }

        private void DeleteInternal(Guid id)
        {
            var path = GetPath(id);

            TryDelete(path);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Storage cleanup must never crash the application.
            }
        }

        private static void MoveToCorrupted(string path)
        {
            try
            {
                Directory.CreateDirectory(CrashStoragePaths.CorruptedDirectory);

                var destination = Path.Combine(CrashStoragePaths.CorruptedDirectory, Path.GetFileName(path));

                File.Move(path, destination, overwrite: true);
            }
            catch
            {
                TryDelete(path);
            }
        }
    }
}
