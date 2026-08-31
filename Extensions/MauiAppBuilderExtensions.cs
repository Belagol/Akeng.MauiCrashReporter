using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Diagnostics;
using AkengMauiCrashReporter.Environments;
using AkengMauiCrashReporter.Formatting;
using AkengMauiCrashReporter.Internal;
using AkengMauiCrashReporter.Lifecycle;
using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Services;
using AkengMauiCrashReporter.Storage;

namespace AkengMauiCrashReporter.Extensions
{
    public static class MauiAppBuilderExtensions
    {
        public static MauiAppBuilder UseAkengMauiCrashReporter(this MauiAppBuilder builder, Action<MauiCrashReporterOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(builder);

            var options = new MauiCrashReporterOptions();

            configure?.Invoke(options);

            options.Validate();

            /*
            * Configuration
            */

            builder.Services.AddSingleton(options);

            builder.Services.AddSingleton(new CrashReportStoreOptions
            {
                MaxStoredReports = options.MaxStoredReports,
                MaxStorageSizeBytes = options.MaxStorageSizeBytes,
                PreserveFatalReports = options.PreserveFatalReports
            });

            /*
             * Core
             */

            builder.Services.AddSingleton(new BreadcrumbStore(options.MaxBreadcrumbs));
            builder.Services.AddSingleton<CrashSessionManager>();
            builder.Services.AddSingleton<CrashExceptionMapper>();
            builder.Services.AddSingleton<CrashFingerprintGenerator>();
            builder.Services.AddSingleton<CrashClassifier>();
            builder.Services.AddSingleton<CrashDiagnosticEngine>();
            builder.Services.AddSingleton<CrashReportFactory>();

            /*
             * Storage
             */

            builder.Services.AddSingleton<EmergencyCrashWriter>();
            builder.Services.AddSingleton<IPendingCrashStore, PendingCrashStore>();
            builder.Services.AddSingleton<ICrashReportStore, CrashReportStore>();
            builder.Services.AddSingleton<CrashReportPresenter>();

            /*
             * Diagnostics
             */

            builder.Services.AddSingleton<CrashLoopDetector>();
            builder.Services.AddSingleton<EnvironmentSnapshotProvider>();
            builder.Services.AddSingleton<CrashReportFormatter>();

            /*
             * Lifecycle
             */

            builder.Services.AddSingleton<MauiLifecycleTracker>();

            /*
             * Main reporter
             */

            builder.Services.AddSingleton<CrashReporter>();
            builder.Services.AddSingleton<ICrashReporter>(provider => provider.GetRequiredService<CrashReporter>());

            /*
             * Exception handling
             */

            builder.Services.AddSingleton<GlobalExceptionHandler>();

            /*
             * Automatic MAUI initialization
             */

            builder.Services.AddSingleton<IMauiInitializeService, CrashReporterMauiInitializer>();

            /*
             * Lifecycle events
             */

            if (options.CaptureLifecycleEvents || options.ShowCrashReportPopupOnNextStartup)
            {
                //WriteDiagnostic("CALLING MauiLifecycleRegistration.Register");
                MauiLifecycleRegistration.Register(builder, options);
            }
            //else
            //{
            //    WriteDiagnostic("LIFECYCLE REGISTRATION SKIPPED");
            //}


            //builder.Services.AddSingleton(options);

            //builder.Services.AddSingleton(new CrashReportStoreOptions
            //{
            //    MaxStoredReports = options.MaxStoredReports,
            //    MaxStorageSizeBytes = options.MaxStorageSizeBytes,
            //    PreserveFatalReports = options.PreserveFatalReports
            //});

            //builder.Services.AddSingleton(new BreadcrumbStore(options.MaxBreadcrumbs));

            //builder.Services.AddSingleton<CrashSessionManager>();

            //builder.Services.AddSingleton<CrashExceptionMapper>();

            //builder.Services.AddSingleton<CrashFingerprintGenerator>();

            //builder.Services.AddSingleton<CrashClassifier>();

            //builder.Services.AddSingleton<CrashDiagnosticEngine>();

            //builder.Services.AddSingleton<CrashReportFactory>();

            //builder.Services.AddSingleton<EmergencyCrashWriter>();

            //builder.Services.AddSingleton<IPendingCrashStore, PendingCrashStore>();

            //builder.Services.AddSingleton<ICrashReportStore, CrashReportStore>();

            //builder.Services.AddSingleton<CrashLoopDetector>();

            //builder.Services.AddSingleton<EnvironmentSnapshotProvider>();

            //builder.Services.AddSingleton<CrashReportFormatter>();

            //builder.Services.AddSingleton<MauiLifecycleTracker>();

            //builder.Services.AddSingleton<CrashReporter>();

            //builder.Services.AddSingleton<ICrashReporter>(serviceProvider => serviceProvider.GetRequiredService<CrashReporter>());

            //builder.Services.AddSingleton<GlobalExceptionHandler>();

            //builder.Services.AddSingleton<CrashReporterInitializer>();

            //if (options.CaptureLifecycleEvents)
            //{
            //    MauiLifecycleRegistration.Register(builder, options);
            //}

            return builder;
        }

        //private static void WriteDiagnostic(string message)
        //{
        //    try
        //    {
        //        var path = Path.Combine(
        //            FileSystem.AppDataDirectory,
        //            "akeng-lifecycle-debug.txt");

        //        File.AppendAllText(
        //            path,
        //            $"{DateTime.UtcNow:O} | {message}{Environment.NewLine}");
        //    }
        //    catch
        //    {
        //    }
        //}
    }
}
