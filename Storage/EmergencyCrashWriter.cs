using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Models.Enums;
using System.Text.Json;

namespace AkengMauiCrashReporter.Storage
{
    internal sealed class EmergencyCrashWriter
    {
        private const int MaxEmergencyMessageLength = 4096;
        private const int MaxEmergencyExceptionTextLength = 64_000;
        private int _isWriting;

        public void Write(Exception exception, CrashOrigin origin, CrashSessionInfo? session = null)
        {
            ArgumentNullException.ThrowIfNull(exception);

            if (Interlocked.Exchange(ref _isWriting, 1) == 1)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(CrashStoragePaths.PendingDirectory);

                var report = CreateEmergencyReport(exception, origin, session);

                WriteAtomically(report);
            }
            catch
            {
                // Critical rule:
                // the crash reporter must never throw
                // while handling an application crash.
            }
            finally
            {
                Interlocked.Exchange(ref _isWriting, 0);
            }
        }

        private static EmergencyCrashReport CreateEmergencyReport(Exception exception, CrashOrigin origin, CrashSessionInfo? session = null)
        {
            string? appVersion = null;

            try
            {
                appVersion = AppInfo.VersionString;
            }
            catch
            {
                // AppInfo may be unavailable during
                // very early application startup.
            }

            return new EmergencyCrashReport
            {
                Id = Guid.NewGuid(),
                TimestampUtc = DateTimeOffset.UtcNow,
                Origin = origin,
                ExceptionType = exception.GetType().FullName ?? exception.GetType().Name,
                Message = Truncate(exception.Message, MaxEmergencyMessageLength) ?? string.Empty,
                ExceptionText = Truncate(SafeExceptionToString(exception), MaxEmergencyExceptionTextLength),
                AppVersion = appVersion,
                SessionId = session?.SessionId ?? Guid.Empty,
                SessionStartedAtUtc = session?.StartedAtUtc ?? DateTimeOffset.UtcNow
            };
        }

        private static void WriteAtomically(EmergencyCrashReport report)
        {
            var fileName = $"crash_{report.TimestampUtc:yyyyMMdd_HHmmss_fff}_{report.Id:N}.json";

            var finalPath = Path.Combine(CrashStoragePaths.PendingDirectory, fileName);

            var tempPath = finalPath + ".tmp";

            try
            {
                var json = JsonSerializer.Serialize(report);

                File.WriteAllText(tempPath, json);

                File.Move(tempPath, finalPath, overwrite: true);
            }
            finally
            {
                TryDeleteTempFile(tempPath);
            }
        }

        private static string SafeExceptionToString(Exception exception)
        {
            try
            {
                return exception.ToString();
            }
            catch
            {
                return $"{exception.GetType().FullName}: {exception.Message}";
            }
        }

        private static void TryDeleteTempFile(string path)
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
            }
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            const string suffix = "\n...[truncated by AkengMauiCrashReporter]";

            var availableLength = Math.Max(0, maxLength - suffix.Length);

            return string.Concat(value.AsSpan(0, availableLength), suffix);
        }

        public void CleanupIncompleteWrites()
        {
            try
            {
                var directory = CrashStoragePaths.PendingDirectory;

                if (!Directory.Exists(directory))
                    return;

                foreach (var file in Directory.EnumerateFiles(directory, "*.tmp"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }
    }
}
