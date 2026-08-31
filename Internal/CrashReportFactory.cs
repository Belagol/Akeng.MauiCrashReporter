using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Diagnostics;
using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Models.Enums;

namespace AkengMauiCrashReporter.Internal
{
    internal sealed class CrashReportFactory
    {
        private readonly MauiCrashReporterOptions _options;
        private readonly CrashExceptionMapper _exceptionMapper;
        private readonly CrashFingerprintGenerator _fingerprintGenerator;
        private readonly CrashSessionManager _sessionManager;
        private readonly CrashClassifier _crashClassifier;
        private readonly CrashDiagnosticEngine _diagnosticEngine;

        public CrashReportFactory(MauiCrashReporterOptions options, CrashExceptionMapper exceptionMapper, CrashFingerprintGenerator fingerprintGenerator, CrashSessionManager sessionManager, CrashClassifier crashClassifier, CrashDiagnosticEngine diagnosticEngine)
        {
            _options = options;
            _exceptionMapper = exceptionMapper;
            _fingerprintGenerator = fingerprintGenerator;
            _sessionManager = sessionManager;
            _crashClassifier = crashClassifier;
            _diagnosticEngine = diagnosticEngine;
        }

        public CrashReport Create(Exception exception, CrashSeverity severity, CrashOrigin origin, EnvironmentSnapshot? environment = null, IReadOnlyList<CrashBreadcrumb>? breadcrumbs = null, IReadOnlyDictionary<string, string>? context = null)
        {
            ArgumentNullException.ThrowIfNull(exception);

            var exceptionInfo = _exceptionMapper.Map(exception);
            var classification = _crashClassifier.Classify(exceptionInfo);
            var diagnostic = _diagnosticEngine.Diagnose(exceptionInfo, classification);

            return new CrashReport
            {
                Id = Guid.NewGuid(),
                TimestampUtc = DateTimeOffset.UtcNow,
                Severity = severity,
                Origin = origin,
                Fingerprint = _fingerprintGenerator.Generate(exceptionInfo),
                Exception = exceptionInfo,
                Application =  TryGetApplicationInfo(),
                Device = TryGetDeviceInfo(),
                Navigation = TryGetNavigationInfo(),
                Session = TryGetSessionInfo(),
                Breadcrumbs = breadcrumbs ?? [],
                Context = context ?? new Dictionary<string, string>(),
                Classification = classification,
                Diagnostic = diagnostic,
                Environment = environment,
            };
        }

        public CrashReport Create(EmergencyCrashReport emergencyReport)
        {
            ArgumentNullException.ThrowIfNull(emergencyReport);

            var exceptionInfo = CreateExceptionInfo(emergencyReport);
            var classification = _crashClassifier.Classify(exceptionInfo);
            var diagnostic = _diagnosticEngine.Diagnose(exceptionInfo, classification);

            return new CrashReport
            {
                Id = emergencyReport.Id,
                TimestampUtc = emergencyReport.TimestampUtc,
                Severity = CrashSeverity.Fatal,
                Origin = emergencyReport.Origin,
                Fingerprint = _fingerprintGenerator.Generate(exceptionInfo),
                Exception = exceptionInfo,
                Application = TryGetApplicationInfo(emergencyReport),
                Device = TryGetDeviceInfo(),
                Navigation = null,
                Session = null,
                Breadcrumbs = [],
                Classification = classification,
                Context = new Dictionary<string, string>(),
                Diagnostic = diagnostic
            };
        }
        private CrashExceptionInfo CreateExceptionInfo(EmergencyCrashReport report)
        {
            return new CrashExceptionInfo
            {
                Type = report.ExceptionType,
                Message = report.Message,
                StackTrace = Limit(report.ExceptionText, _options.MaxStackTraceLength)
            };
        }

        private CrashApplicationInfo? TryGetApplicationInfo(EmergencyCrashReport emergencyReport)
        {
            if (!_options.IncludeAppInfo)
                return null;

            try
            {
                return new CrashApplicationInfo
                {
                    Name = AppInfo.Name,
                    PackageName = AppInfo.PackageName,
                    Version = emergencyReport.AppVersion ?? AppInfo.VersionString,
                    Build = AppInfo.BuildString
                };
            }
            catch
            {
                if (string.IsNullOrWhiteSpace(emergencyReport.AppVersion))
                {
                    return null;
                }

                return new CrashApplicationInfo
                {
                    Version = emergencyReport.AppVersion
                };
            }
        }

        private static string? Limit(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            const string suffix = "\n...[truncated by Akeng.MauiCrashReporter]";

            var availableLength = Math.Max(0, maxLength - suffix.Length);

            return string.Concat(value.AsSpan(0, availableLength), suffix);
        }

        private CrashApplicationInfo? TryGetApplicationInfo()
        {
            if (!_options.IncludeAppInfo)
                return null;

            try
            {
                return new CrashApplicationInfo
                {
                    Name = AppInfo.Name,
                    PackageName = AppInfo.PackageName,
                    Version = AppInfo.VersionString,
                    Build = AppInfo.BuildString
                };
            }
            catch
            {
                return null;
            }
        }

        private CrashDeviceInfo? TryGetDeviceInfo()
        {
            if (!_options.IncludeDeviceInfo)
                return null;

            try
            {
                return new CrashDeviceInfo
                {
                    Platform = DeviceInfo.Platform.ToString(),
                    OsVersion = DeviceInfo.VersionString,
                    Model = DeviceInfo.Model,
                    Manufacturer = DeviceInfo.Manufacturer
                };
            }
            catch
            {
                return null;
            }
        }

        private CrashNavigationInfo? TryGetNavigationInfo()
        {
            if (!_options.IncludeNavigationInfo)
                return null;

            try
            {
                var shell = Shell.Current;

                if (shell is null)
                    return null;

                return new CrashNavigationInfo
                {
                    CurrentPage = shell.CurrentPage?.GetType().FullName,

                    CurrentRoute = shell.CurrentState?.Location?.ToString()
                };
            }
            catch
            {
                return null;
            }
        }

        private CrashSessionInfo? TryGetSessionInfo()
        {
            try
            {
                return _sessionManager.GetSessionInfo();
            }
            catch
            {
                return null;
            }
        }
    }
}
