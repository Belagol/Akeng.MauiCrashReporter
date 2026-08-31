using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Services;

namespace AkengMauiCrashReporter.Internal
{
    internal sealed class CrashReporterMauiInitializer : IMauiInitializeService
    {
        public void Initialize(IServiceProvider services)
        {
            ArgumentNullException.ThrowIfNull(services);

            try
            {
                var options = services.GetRequiredService<MauiCrashReporterOptions>();

                var crashReporter = services.GetRequiredService<CrashReporter>();

                /*
                 * Important:
                 * install global handlers FIRST.
                 *
                 * This allows us to capture crashes occurring
                 * very early during application startup.
                 */
                TryStartGlobalExceptionHandler(services, options);

                /*
                 * Initialization involving disk IO is started
                 * independently.
                 *
                 * It must NEVER prevent the host application
                 * from starting.
                 */
                crashReporter.StartInitialization();
            }
            catch
            {
                /*
                 * CRITICAL RULE:
                 *
                 * Failure to initialize AkengMauiCrashReporter
                 * must never prevent the host MAUI application
                 * from starting.
                 */
            }
        }

        private static void TryStartGlobalExceptionHandler(IServiceProvider services, MauiCrashReporterOptions options)
        {
            try
            {
                if (!options.CaptureUnhandledExceptions && !options.CaptureUnobservedTaskExceptions)
                {
                    return;
                }

                var handler = services.GetRequiredService<GlobalExceptionHandler>();

                handler.Start(options.CaptureUnhandledExceptions, options.CaptureUnobservedTaskExceptions);
            }
            catch
            {
                // Never crash host application.
            }
        }
    }
}
