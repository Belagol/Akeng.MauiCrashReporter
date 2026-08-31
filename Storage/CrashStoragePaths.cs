namespace AkengMauiCrashReporter.Storage
{
    internal static class CrashStoragePaths
    {
        public static string RootDirectory => Path.Combine(FileSystem.AppDataDirectory, "Akeng", "MauiCrashReporter");

        public static string PendingDirectory => Path.Combine(RootDirectory, "Pending");

        public static string ReportsDirectory => Path.Combine(RootDirectory, "Reports");

        public static string CorruptedDirectory => Path.Combine(RootDirectory, "Corrupted");
    }
}
