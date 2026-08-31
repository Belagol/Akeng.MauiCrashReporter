using AkengMauiCrashReporter.Models;
using System.Text.Json;

namespace AkengMauiCrashReporter.Storage
{
    internal sealed class PendingCrashStore : IPendingCrashStore
    {
        private static string DirectoryPath => CrashStoragePaths.PendingDirectory;

        public bool HasPendingCrash
        {
            get
            {
                try
                {
                    return Directory.Exists(DirectoryPath) && Directory.EnumerateFiles(DirectoryPath, "*.json", SearchOption.TopDirectoryOnly).Any();
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<IReadOnlyList<EmergencyCrashReport>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(DirectoryPath))
                return [];

            var reports = new List<EmergencyCrashReport>();

            IEnumerable<string> files;

            try
            {
                files = Directory.EnumerateFiles(DirectoryPath, "*.json", SearchOption.TopDirectoryOnly).OrderBy(path => path).ToArray();
            }
            catch
            {
                return [];
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var report = await TryReadAsync(file, cancellationToken);

                if (report is not null)
                {
                    reports.Add(report);
                }
            }

            return reports.OrderBy(x => x.TimestampUtc).ToArray();
        }

        public async Task<EmergencyCrashReport?> GetLatestAsync(CancellationToken cancellationToken = default)
        {
            var reports = await GetAllAsync(cancellationToken);

            return reports.OrderByDescending(x => x.TimestampUtc).FirstOrDefault();
        }

        public Task DeleteAsync(Guid crashId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(DirectoryPath))
                return Task.CompletedTask;

            try
            {
                foreach (var file in Directory.EnumerateFiles(DirectoryPath, "*.json", SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fileName = Path.GetFileName(file);

                    if (!fileName.Contains(crashId.ToString("N"), StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    TryDelete(file);
                }
            }
            catch
            {
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(DirectoryPath))
                return Task.CompletedTask;

            try
            {
                foreach (var file in Directory.EnumerateFiles(DirectoryPath, "*.json", SearchOption.TopDirectoryOnly))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    TryDelete(file);
                }
            }
            catch
            {
            }

            return Task.CompletedTask;
        }

        private static async Task<EmergencyCrashReport?> TryReadAsync(string file, CancellationToken cancellationToken)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file, cancellationToken);

                if (string.IsNullOrWhiteSpace(json))
                    return null;

                return JsonSerializer.Deserialize<EmergencyCrashReport>(json);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                MoveToCorrupted(file);
                return null;
            }
        }

        private static void TryDelete(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch
            {
            }
        }

        private static void MoveToCorrupted(string file)
        {
            try
            {
                Directory.CreateDirectory(CrashStoragePaths.CorruptedDirectory);

                var destination = Path.Combine(CrashStoragePaths.CorruptedDirectory, Path.GetFileName(file));

                File.Move(file, destination, overwrite: true);
            }
            catch
            {
                TryDelete(file);
            }
        }
    }
}
