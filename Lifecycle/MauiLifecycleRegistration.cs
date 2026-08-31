using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Storage;
using Microsoft.Maui.LifecycleEvents;

namespace AkengMauiCrashReporter.Lifecycle
{
    internal static class MauiLifecycleRegistration
    {
        public static void Register(MauiAppBuilder builder, MauiCrashReporterOptions options)
        {
            WriteDiagnostic("REGISTER ENTERED");
            builder.ConfigureLifecycleEvents(events =>
            {
                WriteDiagnostic("CONFIGURE LIFECYCLE EVENTS");
#if ANDROID
               events.AddAndroid(android =>
               {
               WriteDiagnostic("ADD ANDROID EXECUTED");
                    android.OnCreate((activity, bundle) =>
                    {
                     WriteDiagnostic("ANDROID ONCREATE");
                        if (options.CaptureLifecycleEvents)
                        {
                            Track(MauiLifecycleEventType.ApplicationStarted, activity);
                        }
                    });

                    android.OnResume(activity =>
                    {
                     WriteDiagnostic("ANDROID ONRESUME");
                        if (options.CaptureLifecycleEvents)
                        {
                            Track(MauiLifecycleEventType.ApplicationResumed, activity);
                        }

                        _ = TryPresentPreviousCrashAsync();
                    });
                });
#endif

#if IOS || MACCATALYST
                events.AddiOS(ios =>
                {
                    ios.FinishedLaunching((app, launchOptions) =>
                    {
                        Track(MauiLifecycleEventType.ApplicationStarted, app);
                        return true;
                    });

                    ios.OnActivated(application =>
                    {
                        Track(MauiLifecycleEventType.ApplicationResumed, application);
                        _= TryPresentPreviousCrashAsync();
                    });

                    ios.OnResignActivation(application =>
                    {
                        Track(MauiLifecycleEventType.ApplicationPaused, application);
                    });

                    ios.DidEnterBackground(application =>
                    {
                        Track(MauiLifecycleEventType.ApplicationStopping, application);
                    });
                });
#endif

#if WINDOWS
                events.AddWindows(windows =>
                {
                    windows.OnLaunched((window, args) =>
                    {
                        Track(MauiLifecycleEventType.ApplicationStarted, window);
                        _= TryPresentPreviousCrashAsync();
                    });
                });
#endif
            });
        }

        private static async Task TryPresentPreviousCrashAsync()
        {
            WriteDiagnostic(
            "TryPresentPreviousCrashAsync ENTERED");
            try
            {
                await File.WriteAllTextAsync(Path.Combine(FileSystem.AppDataDirectory, "lifecycle-presenter.txt"), $"Lifecycle called at {DateTime.UtcNow:O}");

                var services = IPlatformApplication.Current?.Services;

                if (services is null)
                {
                    WriteDiagnostic("SERVICES NULL");
                    await File.WriteAllTextAsync(Path.Combine(FileSystem.AppDataDirectory, "lifecycle-presenter.txt"), "SERVICES NULL");
                    return;
                }
                WriteDiagnostic("SERVICES FOUND");
                var presenter = services.GetService<CrashReportPresenter>();

                if (presenter is null)
                {
                    WriteDiagnostic("PRESENTER NULL");
                    await File.WriteAllTextAsync(Path.Combine(FileSystem.AppDataDirectory, "lifecycle-presenter.txt"), "PRESENTER NULL");
                    return;
                }
                WriteDiagnostic("PRESENTER FOUND");
                await File.WriteAllTextAsync(Path.Combine(FileSystem.AppDataDirectory, "lifecycle-presenter.txt"), "PRESENTER FOUND");
                await presenter.TryPresentAsync();
                WriteDiagnostic("PRESENTER FINISHED");
            }
            catch (Exception ex)
            {
                WriteDiagnostic(
                $"PRESENTER ERROR: {ex}");
                try
                {
                    await File.WriteAllTextAsync(Path.Combine(FileSystem.AppDataDirectory, "lifecycle-presenter.txt"), ex.ToString());
                }
                catch
                {
                }
            }
            //try
            //{
            //    var services = IPlatformApplication.Current?.Services;

            //    if (services is null)
            //        return;

            //    var presenter = services.GetService<CrashReportPresenter>();

            //    if (presenter is null)
            //        return;

            //    await presenter.TryPresentAsync();
            //}
            //catch
            //{
            //    // Never crash the host application.
            //}
        }

        private static void Track(MauiLifecycleEventType eventType, object? source = null)
        {
            try
            {
                var services = IPlatformApplication.Current?.Services;
                var tracker = services?.GetService<MauiLifecycleTracker>();
                tracker?.Track(eventType, source);
            }
            catch
            {
            }
        }

        private static void WriteDiagnostic(string message)
        {
            try
            {
                var path = Path.Combine(
                    FileSystem.AppDataDirectory,
                    "akeng-lifecycle-debug.txt");

                File.AppendAllText(
                    path,
                    $"{DateTime.UtcNow:O} | {message}{Environment.NewLine}");
            }
            catch
            {
            }
        }
    }
}
