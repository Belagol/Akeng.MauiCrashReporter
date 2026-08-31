using AkengMauiCrashReporter.Models;

namespace AkengMauiCrashReporter.Diagnostics
{
    internal sealed class CrashDiagnosticEngine
    {
        public DiagnosticResult Diagnose(CrashExceptionInfo exception, CrashClassification classification)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(classification);

            return classification.Category switch
            {
                CrashCategory.Xaml => DiagnoseXaml(classification),

                CrashCategory.Trimming => DiagnoseTrimming(classification),

                CrashCategory.Aot => DiagnoseAot(classification),

                CrashCategory.Resource => DiagnoseResource(classification),

                CrashCategory.Binding => DiagnoseBinding(classification),

                CrashCategory.DependencyInjection => DiagnoseDependencyInjection(classification),

                CrashCategory.Serialization => DiagnoseSerialization(classification),

                CrashCategory.Network => DiagnoseNetwork(classification),

                CrashCategory.Database => DiagnoseDatabase(classification),

                CrashCategory.Navigation => DiagnoseNavigation(classification),

                CrashCategory.Memory => DiagnoseMemory(classification),

                CrashCategory.Threading => DiagnoseThreading(classification),

                _ => DiagnoseUnknown(classification)
            };
        }

        private static DiagnosticResult DiagnoseXaml(CrashClassification classification)
        {
            return CreateResult(classification, "A XAML loading or parsing failure is likely.", classification.LikelyCause,
            [
                Action(1, "Check x:Class", "Verify that the XAML x:Class matches the generated code-behind type."),
                Action(2, "Check resources", "Verify StaticResource, DynamicResource and merged ResourceDictionary keys."),
                Action(3, "Check custom controls", "Verify custom control types, handlers and constructors used from XAML."),
                Action( 4, "Compare Debug and Release", "Run the same screen with Release configuration to isolate configuration-specific behavior.")
            ],

            [
                "Check linker/trimming warnings.",
                "Check AOT-related warnings.",
                "Verify resource Build Action settings.",
                "Verify converters and markup extensions.",
                "Check types referenced only from XAML."
            ]);
        }

        private static DiagnosticResult DiagnoseTrimming(CrashClassification classification)
        {
            return CreateResult(classification, "The crash may be caused by the linker removing code required at runtime.", classification.LikelyCause,

            [
                Action(1, "Inspect linker warnings", "Check build output for trimming warnings related to the failing type or assembly."),
                Action(2, "Check reflection", "Look for types or members accessed through reflection or Activator.CreateInstance."),
                Action(3, "Add preservation metadata", "Consider DynamicallyAccessedMembers or DynamicDependency where appropriate."),
                Action(4, "Compare trimmed and untrimmed builds", "Temporarily disable trimming to confirm whether the issue is Release/linker related.")
            ],

            [
                "Check ILLink warnings.",
                "Check reflection usage.",
                "Check dynamically instantiated types.",
                "Check code referenced only by XAML or serialization."
            ]);
        }

        private static DiagnosticResult DiagnoseAot(CrashClassification classification)
        {
            return CreateResult(classification, "The crash may be related to AOT restrictions or runtime code generation.", classification.LikelyCause,

            [
                Action(1, "Inspect AOT warnings", "Review build and publish warnings related to AOT compatibility."),
                Action(2, "Check dynamic code", "Look for APIs relying on runtime code generation or Reflection.Emit."),
                Action(3, "Check reflection", "Verify that dynamically accessed types are preserved.")
            ],

            [
                "Check AOT compatibility warnings.",
                "Check dynamic code usage.",
                "Check reflection-based activation."
            ]);
        }

        private static DiagnosticResult DiagnoseResource(CrashClassification classification)
        {
            return CreateResult(classification, "A resource could not be resolved at runtime.", classification.LikelyCause,

            [
                Action(1, "Check resource key", "Verify that the requested resource key exists."),
                Action(2, "Check ResourceDictionary", "Verify merged dictionaries and their loading order."),
                Action(3,  "Check Build Action", "Verify that images, fonts and other resources use the correct MAUI configuration.")
            ],

            [
                "Check resources referenced only from XAML.",
                "Check Release packaging.",
                "Check case-sensitive resource names."
            ]);
        }

        private static DiagnosticResult DiagnoseBinding(CrashClassification classification)
        {
            return CreateResult(classification, "The crash appears to involve a binding expression.", classification.LikelyCause,
            [
                Action(1, "Check BindingContext", "Verify that the expected BindingContext is available when the binding is evaluated."),
                Action(2, "Check binding path", "Verify property names and nested binding paths."),
                Action(3, "Check converters", "Verify converter implementations and expected value types.")
            ],

            [
                "Check binding diagnostics.",
                "Check lifecycle timing.",
                "Check nullable values and target property types."
            ]);
        }

        private static DiagnosticResult DiagnoseDependencyInjection(CrashClassification classification)
        {
            return CreateResult(classification, "A dependency could not be resolved from the service container.", classification.LikelyCause,
            [
                Action(1, "Check service registration", "Verify that the required service is registered in MauiProgram."),
                Action(2, "Check constructor dependencies", "Verify every dependency in the failing constructor."),
                Action(3, "Check lifetime", "Verify that Singleton, Scoped and Transient lifetimes are appropriate.")
            ],

            [
                "Check services registered only in Debug.",
                "Check platform-specific registrations.",
                "Check trimmed implementations."
            ]);
        }

        private static DiagnosticResult DiagnoseSerialization(CrashClassification classification)
        {
            return CreateResult(classification, "Serialization or deserialization failed.", classification.LikelyCause,
            [
                Action(1, "Inspect payload", "Verify that the incoming JSON or serialized data matches the expected model."),
                Action(2, "Check converters", "Verify custom converters and property mappings."),
                Action(3, "Check source generation", "For Release/AOT builds, verify serializer source-generation configuration.")
            ],

            [
                "Check DTO preservation.",
                "Check source-generated serializers.",
                "Check polymorphic types."
            ]);
        }

        private static DiagnosticResult DiagnoseNetwork(CrashClassification classification)
        {
            return CreateResult(classification, "The crash appears to originate from network communication.", classification.LikelyCause,
            [
                Action(1, "Check connectivity", "Verify network availability on the affected device."),
                Action(2, "Check endpoint", "Verify URL, TLS configuration and server availability."),
                Action(3, "Check timeout/retry policy", "Ensure transient network failures are handled appropriately.")
            ],

            []);
        }

        private static DiagnosticResult DiagnoseDatabase(CrashClassification classification)
        {
            return CreateResult(classification, "A database operation failed.", classification.LikelyCause,
            [
                Action(1, "Check connection", "Verify database connection configuration."),
                Action(2, "Check schema", "Verify migrations and database schema compatibility."),
                Action(3, "Check query", "Inspect the query and parameters involved in the failure.")
            ],

            []);
        }

        private static DiagnosticResult DiagnoseNavigation(CrashClassification classification)
        {
            return CreateResult(classification, "The crash appears to involve application navigation.", classification.LikelyCause,
            [
                Action(1, "Check route", "Verify the Shell route is registered and resolves to the expected page."),
                Action(2, "Check parameters", "Verify navigation parameters and their expected types."),
                Action(3, "Check lifecycle", "Verify navigation does not occur before the page or Shell is initialized.")
            ],

            []);
        }

        private static DiagnosticResult DiagnoseMemory(CrashClassification classification)
        {
            return CreateResult(classification, "The application may have exhausted available memory.", classification.LikelyCause,
            [
                Action(1, "Inspect large allocations", "Look for large images, collections, buffers or unmanaged allocations."),
                Action(2, "Check image sizes", "Avoid loading unnecessarily large images into memory."),
                Action(3, "Check resource lifetime", "Verify streams, native resources and subscriptions are released.")
            ],

            []);
        }

        private static DiagnosticResult DiagnoseThreading(CrashClassification classification)
        {
            return CreateResult(classification, "The crash may involve an invalid thread or concurrency operation.", classification.LikelyCause,
            [
                Action(1, "Check UI thread", "Verify UI operations are executed on the MAUI main thread."),
                Action(2, "Check shared state", "Inspect concurrent access to mutable state."),
                Action(3, "Check synchronization", "Review locks, cancellation and task coordination.")
            ],

            []);
        }

        private static DiagnosticResult DiagnoseUnknown(CrashClassification classification)
        {
            return CreateResult(classification, "The crash could not be classified with sufficient confidence.", classification.LikelyCause,
            [
                Action(1, "Inspect stack trace", "Start with the first application-owned frame in the stack trace."),
                Action(2, "Check inner exception", "Inspect the complete exception chain for the underlying cause.")
            ],

            []);
        }

        private static DiagnosticResult CreateResult(CrashClassification classification, string summary, string? likelyCause, IReadOnlyList<DiagnosticAction> actions, IReadOnlyList<string> releaseChecks)
        {
            return new DiagnosticResult
            {
                Classification = classification,
                Summary = summary,
                LikelyCause = likelyCause,
                RecommendedActions = actions,
                ReleaseChecks = releaseChecks,
                RequiresDeveloperAttention = classification.Category != CrashCategory.Unknown
            };
        }

        private static DiagnosticAction Action(int priority, string title, string description)
        {
            return new DiagnosticAction
            {
                Priority = priority,
                Title = title,
                Description = description
            };
        }
    }
}
