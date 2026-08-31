using AkengMauiCrashReporter.Models;
using System.Text;
using System.Text.Json;

namespace AkengMauiCrashReporter.Formatting
{
    public sealed class CrashReportFormatter
    {
        private readonly CrashReportFormattingOptions _options;

        public CrashReportFormatter(CrashReportFormattingOptions? options = null)
        {
            _options = options ?? new CrashReportFormattingOptions();
        }

        public string Format(CrashReport report, CrashReportFormat format)
        {
            ArgumentNullException.ThrowIfNull(report);

            return format switch
            {
                CrashReportFormat.Text => FormatText(report),

                CrashReportFormat.Markdown => FormatMarkdown(report),

                CrashReportFormat.Json => FormatJson(report),

                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
        }

        private string FormatText(CrashReport report)
        {
            var builder = new StringBuilder();

            builder.AppendLine("========================================");

            builder.AppendLine("       AKENG MAUI CRASH REPORT");

            builder.AppendLine("========================================");

            builder.AppendLine();

            AppendBasicInformation(builder, report);

            if (_options.IncludeDiagnostic)
            {
                AppendDiagnostic(builder, report);
            }

            if (_options.IncludeSession)
            {
                AppendSession(builder, report);
            }

            if (_options.IncludeEnvironment)
            {
                AppendEnvironment(builder, report);
            }

            if (_options.IncludeBreadcrumbs)
            {
                AppendBreadcrumbs(builder, report);
            }

            if (_options.IncludeStackTrace)
            {
                AppendStackTrace(builder, report);
            }

            return builder.ToString();
        }

        private string FormatMarkdown(CrashReport report)
        {
            var builder = new StringBuilder();

            builder.AppendLine("# 🚨 MAUI Crash Report");

            builder.AppendLine();

            builder.AppendLine("## Crash");

            builder.AppendLine();

            builder.AppendLine($"**Exception:** `{report.Exception.Type}`");

            builder.AppendLine($"**Severity:** `{report.Severity}`");

            builder.AppendLine($"**Origin:** `{report.Origin}`");

            if (_options.IncludeFingerprint && !string.IsNullOrWhiteSpace(report.Fingerprint))
            {
                builder.AppendLine($"**Fingerprint:** `{report.Fingerprint}`");
            }

            builder.AppendLine();

            if (_options.IncludeDiagnostic)
            {
                AppendDiagnosticMarkdown(builder, report);
            }

            if (_options.IncludeEnvironment)
            {
                AppendEnvironmentMarkdown(builder, report);
            }

            if (_options.IncludeSession)
            {
                AppendSessionMarkdown(builder, report);
            }

            if (_options.IncludeBreadcrumbs)
            {
                AppendBreadcrumbsMarkdown(builder, report);
            }

            if (_options.IncludeStackTrace)
            {
                builder.AppendLine("## Stack Trace");

                builder.AppendLine();

                builder.AppendLine("```text");

                builder.AppendLine(report.Exception.StackTrace);

                builder.AppendLine("```");

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string FormatJson(CrashReport report)
        {
            return JsonSerializer.Serialize(report,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private void AppendBasicInformation(StringBuilder builder, CrashReport report)
        {
            builder.AppendLine("CRASH");
            builder.AppendLine("----------------------------------------");

            builder.AppendLine($"Exception : {report.Exception.Type}");

            builder.AppendLine($"Message   : {report.Exception.Message}");

            builder.AppendLine($"Severity  : {report.Severity}");

            builder.AppendLine($"Origin    : {report.Origin}");

            if (_options.IncludeFingerprint && !string.IsNullOrWhiteSpace(report.Fingerprint))
            {
                builder.AppendLine($"Fingerprint : {report.Fingerprint}");
            }

            builder.AppendLine();
        }

        private static void AppendDiagnostic(StringBuilder builder, CrashReport report)
        {
            var diagnostic = report.Diagnostic;

            if (diagnostic is null)
                return;

            var classification = diagnostic.Classification;

            builder.AppendLine("DIAGNOSTIC");

            builder.AppendLine("----------------------------------------");

            builder.AppendLine($"Category     : {classification.Category}");

            builder.AppendLine($"Confidence   : {classification.Confidence:P0}");

            builder.AppendLine($"Score        : {classification.Score}");

            builder.AppendLine($"Release issue: {diagnostic.IsLikelyReleaseSpecific}");

            builder.AppendLine();

            builder.AppendLine($"Summary: {diagnostic.Summary}");

            if (!string.IsNullOrWhiteSpace(diagnostic.LikelyCause))
            {
                builder.AppendLine();

                builder.AppendLine($"Likely cause: {diagnostic.LikelyCause}");
            }

            if (diagnostic.RecommendedActions.Count > 0)
            {
                builder.AppendLine();

                builder.AppendLine("Recommended actions:");

                foreach (var action in diagnostic.RecommendedActions.OrderBy(x => x.Priority))
                {
                    builder.AppendLine($"{action.Priority}. {action.Title}");

                    builder.AppendLine($"   {action.Description}");
                }
            }

            if (diagnostic.ReleaseChecks.Count > 0)
            {
                builder.AppendLine();

                builder.AppendLine("Release checks:");

                foreach (var check in diagnostic.ReleaseChecks)
                {
                    builder.AppendLine($" - {check}");
                }
            }

            if (classification.Evidence.Count > 0)
            {
                builder.AppendLine();

                builder.AppendLine("Evidence:");

                foreach (var evidence in classification.Evidence)
                {
                    builder.AppendLine($" - {evidence}");
                }
            }

            builder.AppendLine();
        }

        private static void AppendSession(StringBuilder builder, CrashReport report)
        {
            var session = report.Session;

            if (session is null)
                return;

            builder.AppendLine("SESSION");

            builder.AppendLine("----------------------------------------");

            builder.AppendLine($"Session ID : {session.SessionId}");

            builder.AppendLine($"Started    : {session.StartedAtUtc:O}");

            builder.AppendLine($"Duration   : {session.Duration}");

            builder.AppendLine();
        }

        private static void AppendEnvironment(StringBuilder builder, CrashReport report)
        {
            var environment = report.Environment;

            if (environment is null)
                return;

            builder.AppendLine("ENVIRONMENT");

            builder.AppendLine("----------------------------------------");

            builder.AppendLine($"Platform       : {environment.Platform}");

            builder.AppendLine($"OS             : {environment.OperatingSystem}");

            builder.AppendLine($"OS Version     : {environment.OperatingSystemVersion}");

            builder.AppendLine($"Architecture   : {environment.Architecture}");

            builder.AppendLine($"Device         : {environment.DeviceModel}");

            builder.AppendLine($"App Version    : {environment.AppVersion}");

            builder.AppendLine($"App Build      : {environment.AppBuild}");

            builder.AppendLine($".NET           : {environment.DotNetVersion}");

            builder.AppendLine($"MAUI           : {environment.MauiVersion}");

            builder.AppendLine($"Configuration  : {environment.BuildConfiguration}");

            builder.AppendLine($"Release        : {environment.IsRelease}");

            builder.AppendLine($"Debugger       : {environment.IsDebuggerAttached}");

            builder.AppendLine($"Trimming       : {environment.IsTrimmingEnabled}");

            builder.AppendLine($"AOT            : {environment.IsAotEnabled}");

            builder.AppendLine();
        }

        private void AppendBreadcrumbs(StringBuilder builder, CrashReport report)
        {
            var breadcrumbs = report.Breadcrumbs;

            if (breadcrumbs is null || breadcrumbs.Count == 0)
            {
                return;
            }

            builder.AppendLine("LIFECYCLE / BREADCRUMBS");

            builder.AppendLine("----------------------------------------");

            foreach (var breadcrumb in breadcrumbs.TakeLast(_options.MaxBreadcrumbs))
            {
                builder.AppendLine($"[{breadcrumb.TimestampUtc:HH:mm:ss.fff}] " +
                    $"[{breadcrumb.Category}] " + breadcrumb.Message);
            }

            builder.AppendLine();
        }

        private static void AppendStackTrace(StringBuilder builder, CrashReport report)
        {
            if (string.IsNullOrWhiteSpace(report.Exception.StackTrace))
            {
                return;
            }

            builder.AppendLine("STACK TRACE");

            builder.AppendLine("----------------------------------------");

            builder.AppendLine(report.Exception.StackTrace);

            builder.AppendLine();
        }

        private static void AppendDiagnosticMarkdown(StringBuilder builder, CrashReport report)
        {
            var diagnostic = report.Diagnostic;

            if (diagnostic is null)
                return;

            var classification = diagnostic.Classification;

            builder.AppendLine("## Diagnostic");

            builder.AppendLine();

            builder.AppendLine($"**Category:** `{classification.Category}`");

            builder.AppendLine($"**Confidence:** `{classification.Confidence:P0}`");

            builder.AppendLine($"**Release-sensitive:** `{diagnostic.IsLikelyReleaseSpecific}`");

            builder.AppendLine();

            builder.AppendLine($"### Summary");

            builder.AppendLine();

            builder.AppendLine(diagnostic.Summary);

            if (!string.IsNullOrWhiteSpace(diagnostic.LikelyCause))
            {
                builder.AppendLine();

                builder.AppendLine($"### Likely cause");

                builder.AppendLine();

                builder.AppendLine(diagnostic.LikelyCause);
            }

            if (diagnostic.RecommendedActions.Count > 0)
            {
                builder.AppendLine();

                builder.AppendLine("### Recommended actions");

                builder.AppendLine();

                foreach (var action in diagnostic.RecommendedActions.OrderBy(x => x.Priority))
                {
                    builder.AppendLine($"{action.Priority}. **{action.Title}** — " + action.Description);
                }
            }

            if (classification.Evidence.Count > 0)
            {
                builder.AppendLine();

                builder.AppendLine("### Evidence");

                builder.AppendLine();

                foreach (var evidence in classification.Evidence)
                {
                    builder.AppendLine($"- {evidence}");
                }
            }

            builder.AppendLine();
        }

        private static void AppendEnvironmentMarkdown(StringBuilder builder, CrashReport report)
        {
            var environment = report.Environment;

            if (environment is null)
                return;

            builder.AppendLine("## Environment");

            builder.AppendLine();

            builder.AppendLine("| Property | Value |");

            builder.AppendLine("|---|---|");

            builder.AppendLine($"| Platform | {environment.Platform} |");

            builder.AppendLine($"| OS | {environment.OperatingSystem} |");

            builder.AppendLine($"| OS Version | {environment.OperatingSystemVersion} |");

            builder.AppendLine($"| Architecture | {environment.Architecture} |");

            builder.AppendLine($"| Device | {environment.DeviceModel} |");

            builder.AppendLine($"| App Version | {environment.AppVersion} |");

            builder.AppendLine($"| Build | {environment.AppBuild} |");

            builder.AppendLine($"| .NET | {environment.DotNetVersion} |");

            builder.AppendLine($"| MAUI | {environment.MauiVersion} |");

            builder.AppendLine($"| Configuration | {environment.BuildConfiguration} |");

            builder.AppendLine($"| Trimming | {environment.IsTrimmingEnabled} |");

            builder.AppendLine($"| AOT | {environment.IsAotEnabled} |");

            builder.AppendLine();
        }


        private static void AppendSessionMarkdown(StringBuilder builder, CrashReport report)
        {
            var session = report.Session;

            if (session is null)
                return;

            builder.AppendLine("## Session");

            builder.AppendLine();

            builder.AppendLine($"**Session ID:** `{session.SessionId}`");

            builder.AppendLine($"**Started:** `{session.StartedAtUtc:O}`");

            builder.AppendLine($"**Duration:** `{session.Duration}`");

            builder.AppendLine();
        }

        private void AppendBreadcrumbsMarkdown(StringBuilder builder, CrashReport report)
        {
            var breadcrumbs = report.Breadcrumbs;

            if (breadcrumbs is null ||
                breadcrumbs.Count == 0)
            {
                return;
            }

            builder.AppendLine("## Breadcrumbs");

            builder.AppendLine();

            builder.AppendLine("```text");

            foreach (var breadcrumb in breadcrumbs.TakeLast(_options.MaxBreadcrumbs))
            {
                builder.AppendLine($"[{breadcrumb.TimestampUtc:HH:mm:ss.fff}] " +
                    $"[{breadcrumb.Category}] " + breadcrumb.Message);
            }

            builder.AppendLine("```");

            builder.AppendLine();
        }

    }
}
