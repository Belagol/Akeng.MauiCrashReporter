namespace AkengMauiCrashReporter.Models
{
    public sealed class EnvironmentSnapshot
    {
        public string Platform { get; init; } = string.Empty;

        public string OperatingSystem { get; init; } = string.Empty;

        public string OperatingSystemVersion { get; init; } = string.Empty;

        public string Architecture { get; init; } = string.Empty;

        public string DeviceModel { get; init; } = string.Empty;

        public string AppVersion { get; init; } = string.Empty;

        public string AppBuild { get; init; } = string.Empty;

        public string DotNetVersion { get; init; } = string.Empty;

        public string MauiVersion { get; init; } = string.Empty;

        public string BuildConfiguration { get; init; } = string.Empty;

        public bool IsRelease { get; init; }

        public bool IsDebuggerAttached { get; init; }

        public bool? IsTrimmingEnabled { get; init; }

        public bool? IsAotEnabled { get; init; }
    }
}
