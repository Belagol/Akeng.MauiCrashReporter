using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Models;
using System.Diagnostics;

namespace AkengMauiCrashReporter.Environments
{
    internal sealed class EnvironmentSnapshotProvider
    {
        private readonly MauiCrashReporterOptions _options;

        public EnvironmentSnapshotProvider(MauiCrashReporterOptions options)
        {
            _options = options;
        }

        public EnvironmentSnapshot Capture()
        {
            return new EnvironmentSnapshot
            {
                Platform = GetPlatform(),
                OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                OperatingSystemVersion = Environment.OSVersion.VersionString,
                Architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString(),
                DeviceModel = GetDeviceModel(),
                AppVersion = AppInfo.Current.VersionString,
                AppBuild = AppInfo.Current.BuildString,
                DotNetVersion = Environment.Version.ToString(),
                MauiVersion = GetMauiVersion(),
                BuildConfiguration = GetBuildConfiguration(),
                IsRelease = !IsDebugBuild(),
                IsDebuggerAttached = Debugger.IsAttached,
                IsTrimmingEnabled = _options.TrimmingEnabled ?? false,
                IsAotEnabled = _options.AotEnabled ?? false
            };
        }

        private static string GetPlatform()
        {
#if ANDROID
            return "Android";
#elif IOS
            return "iOS";
#elif MACCATALYST
            return "MacCatalyst";
#elif WINDOWS
            return "Windows";
#elif TIZEN
            return "Tizen";
#else
            return "Unknown";
#endif
        }

        private static string GetDeviceModel()
        {
#if ANDROID
            return Android.OS.Build.Model ?? "Unknown";
#elif IOS || MACCATALYST
            return UIKit.UIDevice.CurrentDevice.Model ?? "Unknown";
#else
            return "Unknown";
#endif
        }

        private static string GetMauiVersion()
        {
            var assembly = typeof(Microsoft.Maui.Controls.Application).Assembly;

            return assembly.GetName().Version?.ToString() ?? "Unknown";
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }

        private static bool IsDebugBuild()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
