namespace AkengMauiCrashReporter.Diagnostics
{
    public enum CrashCategory
    {
        Unknown = 0,

        Xaml = 1,

        Binding = 2,

        Resource = 3,

        Trimming = 4,

        Aot = 5,

        Reflection = 6,

        Serialization = 7,

        Navigation = 8,

        Network = 9,

        Database = 10,

        DependencyInjection = 11,

        Platform = 12,

        Memory = 13,

        Threading = 14
    }
}
