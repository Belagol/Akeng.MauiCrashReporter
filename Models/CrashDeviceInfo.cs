namespace AkengMauiCrashReporter.Models
{
    public sealed class CrashDeviceInfo
    {
        public string? Platform { get; init; }

        public string? OsVersion { get; init; }

        public string? Model { get; init; }

        public string? Manufacturer { get; init; }
    }
}
