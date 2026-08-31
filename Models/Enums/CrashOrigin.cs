namespace AkengMauiCrashReporter.Models.Enums
{
    public enum CrashOrigin
    {
        Unknown = 0,
        Manual = 1,
        AppDomainUnhandledException = 2,
        UnobservedTaskException = 3,
        AndroidUnhandledException = 4,
        iOSUnhandledException = 5,
        Startup = 6,
        WindowsUnhandledException = 7,
        MauiDispatcherUnhandledException = 8,
        Native = 9
    }
}
