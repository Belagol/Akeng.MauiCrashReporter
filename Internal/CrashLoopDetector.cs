using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Models.Enums;

namespace AkengMauiCrashReporter.Internal
{
    internal sealed class CrashLoopDetector
    {
        private readonly MauiCrashReporterOptions _options;

        public CrashLoopDetector(MauiCrashReporterOptions options)
        {
            _options = options;
        }

        public CrashLoopInfo Analyze(IReadOnlyList<CrashReport> reports)
        {
            if (reports is null || reports.Count == 0)
            {
                return new CrashLoopInfo
                {
                    IsDetected = false
                };
            }

            var fatalReports = reports
                    .Where(IsRelevantCrash)
                    .OrderByDescending(x => x.TimestampUtc)
                    .ToArray();

            if (fatalReports.Length < _options.CrashLoopThreshold)
            {
                return new CrashLoopInfo
                {
                    IsDetected = false
                };
            }

            var groups = fatalReports
                    .Where(x => !string.IsNullOrWhiteSpace(x.Fingerprint))
                    .GroupBy(x => x.Fingerprint)
                    .ToArray();

            foreach (var group in groups)
            {
                var ordered = group
                        .OrderByDescending(x => x.TimestampUtc)
                        .ToArray();

                if (ordered.Length < _options.CrashLoopThreshold)
                {
                    continue;
                }

                var latest = ordered[0];

                var oldestRelevant = ordered[_options.CrashLoopThreshold - 1];

                var window = latest.TimestampUtc - oldestRelevant.TimestampUtc;

                if (window > _options.CrashLoopWindow)
                {
                    continue;
                }

                var sessions = ordered
                        .Select(x => x.Session?.SessionId)
                        .Where(x => x.HasValue && x.Value != Guid.Empty)
                        .Select(x => x!.Value)
                        .Distinct()
                        .ToArray();

                if (sessions.Length < _options.CrashLoopThreshold)
                {
                    continue;
                }

                var startupCrash = ordered.Take(_options.CrashLoopThreshold)
                    .All(report => IsStartupCrash(report, _options.StartupCrashThreshold));

                return new CrashLoopInfo
                {
                    IsDetected = true,
                    Fingerprint = group.Key,
                    Occurrences = _options.CrashLoopThreshold,
                    Window = window,
                    IsStartupCrash = startupCrash,
                    SessionIds = sessions
                };
            }

            return new CrashLoopInfo
            {
                IsDetected = false
            };
        }

        private static bool IsRelevantCrash(CrashReport report)
        {
            return report.Severity == CrashSeverity.Fatal && !string.IsNullOrWhiteSpace(report.Fingerprint);
        }

        private static bool IsStartupCrash(CrashReport report, TimeSpan threshold)
        {
            if (report.Session is null)
                return false;

            return report.Session.Duration <= threshold;
        }
    }
}
