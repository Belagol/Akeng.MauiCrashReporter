using AkengMauiCrashReporter.Models.Enums;
using AkengMauiCrashReporter.Services;
#if ANDROID
using Android.Runtime;
#endif

namespace AkengMauiCrashReporter.Internal
{
    internal sealed class GlobalExceptionHandler : IDisposable
    {
        private readonly CrashReporter _crashReporter;

        private int _started;
        private int _disposed;

        public GlobalExceptionHandler(CrashReporter crashReporter)
        {
            _crashReporter = crashReporter ?? throw new ArgumentNullException(nameof(crashReporter));
        }

        public void Start(bool captureUnhandledExceptions, bool captureUnobservedTaskExceptions)
        {
            if (Volatile.Read(ref _disposed) == 1)
                return;

            if (Interlocked.Exchange(ref _started, 1) == 1)
            {
                return;
            }

            if (captureUnhandledExceptions)
            {
                try
                {
                    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                }
                catch
                {
                }

#if ANDROID
                try
                {
                    AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidUnhandledException;
                }
                catch
                {
                }
#endif
            }

            if (captureUnobservedTaskExceptions)
            {
                try
                {
                    TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
                }
                catch
                {
                }
            }
        }

        private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is not Exception exception)
                return;

            try
            {
                _crashReporter.CaptureFatalException(exception, CrashOrigin.AppDomainUnhandledException);
            }
            catch
            {
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception is null)
                return;

            try
            {
                _= _crashReporter.CaptureExceptionAsync(e.Exception, CrashOrigin.UnobservedTaskException, CrashSeverity.NonFatal);

                e.SetObserved();
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            if (Interlocked.Exchange(ref _started, 0) == 1)
            {
                try
                {
                    AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
                }
                catch
                {
                }

#if ANDROID
                try
                {
                    AndroidEnvironment.UnhandledExceptionRaiser -=
                        OnAndroidUnhandledException;
                }
                catch
                {
                }
#endif

                try
                {
                    TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
                }
                catch
                {
                }
            }
        }

#if ANDROID
        private void OnAndroidUnhandledException(object? sender, RaiseThrowableEventArgs e)
        {
            try
            {
                if (e.Exception is null)
                    return;

                _crashReporter.CaptureFatalException(e.Exception, CrashOrigin.AndroidUnhandledException);
            }
            catch
            {
                // Never crash because of the reporter itself.
            }
        }
#endif
    }
}
