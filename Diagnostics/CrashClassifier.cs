using AkengMauiCrashReporter.Models;

namespace AkengMauiCrashReporter.Diagnostics
{
    internal sealed class CrashClassifier
    {
        private readonly IReadOnlyList<CrashClassificationRule> _rules;

        public CrashClassifier()
        {
            _rules = BuildRules();
        }

        public CrashClassification Classify(CrashExceptionInfo exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            var type = exception.Type ?? string.Empty;

            var message = exception.Message ?? string.Empty;

            var stack = exception.StackTrace;

            var matches = new List<(CrashClassificationRule Rule, CrashRuleMatch Match)>();

            foreach (var rule in _rules)
            {
                try
                {
                    var match = rule.Match(type, message, stack);

                    if (match is null || match.Score <= 0)
                    {
                        continue;
                    }

                    matches.Add((rule, match));
                }
                catch
                {
                    // A diagnostic rule must never
                    // break crash reporting.
                }
            }

            if (matches.Count == 0)
            {
                return CreateUnknownClassification(type, message, stack);
            }

            var best = matches.OrderByDescending(x => x.Match.Score).First();

            var totalScore = matches.Sum(x => x.Match.Score);

            var confidence = CalculateConfidence(best.Match.Score, totalScore);

            var evidence = matches
                    .OrderByDescending(x => x.Match.Score)
                    .SelectMany(x => x.Match.Evidence)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(10)
                    .ToArray();

            return new CrashClassification
            {
                Category = best.Rule.Category,

                Title = best.Rule.Title,

                Description = best.Rule.Description,

                LikelyCause = best.Rule.LikelyCause,

                Recommendation = best.Rule.Recommendation,

                Confidence = confidence,

                Score = best.Match.Score,

                IsReleaseSensitive = matches.Any(x => x.Rule.IsReleaseSensitive && x.Match.Score >= 3),

                Evidence = evidence
            };
        }

        private static double CalculateConfidence(int bestScore, int totalScore)
        {
            if (bestScore <= 0 || totalScore <= 0)
            {
                return 0;
            }

            var confidence = (double)bestScore / totalScore;

            return Math.Round(Math.Min(confidence, 1.0), 2);
        }

        private static CrashClassification CreateUnknownClassification(string type, string message, string? stack)
        {
            return new CrashClassification
            {
                Category = CrashCategory.Unknown,

                Title = "Unknown application crash",

                Description = "The crash could not be confidently classified.",

                Confidence = 0,

                Score = 0,

                IsReleaseSensitive = false,

                Evidence = BuildBasicEvidence(type, message, stack)
            };
        }

        private static IReadOnlyList<CrashClassificationRule> BuildRules()
        {
            return
            [
                XamlRule(),
                BindingRule(),
                ResourceRule(),
                TrimmingRule(),
                AotRule(),
                ReflectionRule(),
                SerializationRule(),
                NavigationRule(),
                NetworkRule(),
                DatabaseRule(),
                DependencyInjectionRule(),
                MemoryRule(),
                ThreadingRule()
            ];
        }

        private static CrashClassificationRule XamlRule()
        {
            return new CrashClassificationRule(CrashCategory.Xaml, "XAML loading/parsing failure", "The exception appears to originate from XAML loading or parsing.", "A XAML type, resource, property, converter or markup extension may not be available at runtime.", "Check x:Class, resources, converters, custom controls and Release/AOT behavior.", true, (type, message, stack) =>
            {
                var score = 0;

                var evidence = new List<string>();

                if (Contains(type, "XamlParseException", "XamlLoadException", "XamlException"))
                {
                    score += 5;
                    evidence.Add("XAML exception type detected.");
                }

                if (Contains(stack, "XamlLoader", "XamlParse", "ApplyProperties"))
                {
                    score += 4;
                    evidence.Add("MAUI XAML loading/parsing code detected.");
                }

                if (Contains(message, "XAML", "Xaml"))
                {
                    score += 2;
                    evidence.Add("XAML indicator found in exception message.");
                }

                if (Contains(message, "Cannot assign", "Cannot set", "Failed to load"))
                {
                    score += 1;
                    evidence.Add("Possible XAML property loading failure.");
                }

                return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
            });
        }

        private static CrashClassificationRule BindingRule()
        {
            return new CrashClassificationRule(CrashCategory.Binding, "Binding-related failure", "The exception appears to involve a MAUI binding or binding expression.", "A binding path, source object, converter, BindingContext or target property may be invalid.", "Check binding paths, BindingContext values, converters, target property types and compiled bindings.", false, (type, message, stack) =>
            {
                var score = 0;
                var evidence = new List<string>();

                if (Contains(type, "BindingException", "BindingExpressionException"))
                {
                    score += 5;
                    evidence.Add("Binding-related exception type detected.");
                }

                if (Contains(stack, "BindingExpression", "BindingBase", "BindableObject", "SetBinding", "ApplyBindings"))
                {
                    score += 4;
                    evidence.Add("MAUI binding infrastructure detected in stack trace.");
                }

                if (Contains(message, "Binding", "binding expression", "BindingContext", "binding path"))
                {
                    score += 3;
                    evidence.Add("Binding indicator found in exception message.");
                }

                if (Contains(message, "property not found", "cannot convert", "cannot assign", "target property", "source property"))
                {
                    score += 2;
                    evidence.Add("Possible binding source or target mismatch detected.");
                }

                if (Contains(message, "converter", "IValueConverter"))
                {
                    score += 2;
                    evidence.Add("Binding converter involvement detected.");
                }

                return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
            });
        }

        private static CrashClassificationRule ResourceRule()
        {
            return new CrashClassificationRule(CrashCategory.Resource, "Resource lookup failure", "The application appears to be unable to resolve a MAUI resource.", "A StaticResource, DynamicResource, image, font, style or ResourceDictionary entry may be missing or unavailable at runtime.", "Verify resource keys, merged dictionaries, Build Action settings, image/font registration and Release trimming behavior.", true, (type, message, stack) =>
           {
               var score = 0;
               var evidence = new List<string>();

               if (Contains(type, "ResourceNotFoundException", "KeyNotFoundException"))
               {
                   score += 4;
                   evidence.Add("Resource-related exception type detected.");
               }

               if (Contains(message, "StaticResource", "DynamicResource"))
               {
                   score += 5;
                   evidence.Add("StaticResource or DynamicResource lookup detected.");
               }

               if (Contains(message, "ResourceDictionary", "resource not found", "resource could not be found", "resource was not found"))
               {
                   score += 4;
                   evidence.Add("Resource lookup failure detected in exception message.");
               }

               if (Contains(stack, "ResourceDictionary", "TryGetResource", "GetResource", "SetDynamicResource"))
               {
                   score += 4;
                   evidence.Add("MAUI resource lookup infrastructure detected in stack trace.");
               }

               if (Contains(message, "font", "image", "drawable", "mauiimage", "mauifont"))
               {
                   score += 2;
                   evidence.Add("Possible image or font resource issue detected.");
               }

               if (Contains(message, "style", "controltemplate", "datatemplate"))
               {
                   score += 2;
                   evidence.Add("Possible style or template resource issue detected.");
               }

               return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
           });
        }

        private static CrashClassificationRule TrimmingRule()
        {
            return new CrashClassificationRule(CrashCategory.Trimming, "Possible trimming/linker issue", "The exception may be caused by code removed by the linker during Release publishing.", "A type, member or constructor required through reflection may have been trimmed.", "Check linker warnings, DynamicallyAccessedMembers, DynamicDependency and reflection usage.", true, (type, message, stack) =>
             {
                 var score = 0;

                 var evidence = new List<string>();

                 if (Contains(type, "MissingMethodException"))
                 {
                     score += 5;
                     evidence.Add("MissingMethodException detected.");
                 }

                 if (Contains(type, "MissingMemberException"))
                 {
                     score += 5;
                     evidence.Add("MissingMemberException detected.");
                 }

                 if (Contains(type, "TypeLoadException"))
                 {
                     score += 4;
                     evidence.Add("TypeLoadException detected.");
                 }

                 if (Contains(message, "Could not load type", "Missing method", "Missing member"))
                 {
                     score += 3;
                     evidence.Add("Missing runtime type/member indicator detected.");
                 }

                 if (Contains(stack, "System.Reflection", "Activator.CreateInstance"))
                 {
                     score += 3;
                     evidence.Add("Reflection-based activation detected.");
                 }

                 return score == 0 ? null : new CrashRuleMatch
                 {
                    Score = score,
                    Evidence = evidence
                 };
             });

            //return new CrashClassificationRule(
            //    CrashCategory.Trimming,

            //    "Possible trimming/linker issue",

            //    "The exception may be caused by code removed by the linker during Release publishing.",

            //    "A type, member or constructor required through reflection may have been trimmed.",

            //    "Check linker warnings, DynamicallyAccessedMembers, DynamicDependency, Preserve attributes and reflection usage.",

            //    0.90,

            //    true,

            //    (type, message, stack) =>
            //        ContainsAny(
            //            type,
            //            "MissingMethodException",
            //            "MissingMemberException",
            //            "TypeLoadException")
            //        ||
            //        ContainsAny(
            //            message,
            //            "Could not load type",
            //            "Missing method",
            //            "Missing member",
            //            "constructor")
            //        ||
            //        ContainsAny(
            //            stack,
            //            "Microsoft.Extensions.DependencyInjection",
            //            "Activator.CreateInstance",
            //            "System.Reflection"));
        }

        private static CrashClassificationRule AotRule()
        {
            return new CrashClassificationRule(CrashCategory.Aot, "Possible AOT/runtime compatibility issue", "The exception may be related to ahead-of-time compilation or runtime code generation.", "Runtime code generation, reflection or dynamically created members may not be compatible with the selected AOT configuration.", "Check AOT compatibility warnings, reflection usage, dynamic code generation and trimming annotations.", true, (type, message, stack) =>
           {
               var score = 0;
               var evidence = new List<string>();

               if (Contains(message, "AOT", "ahead-of-time", "NativeAOT"))
               {
                   score += 5;
                   evidence.Add("AOT-related indicator detected in exception message.");
               }

               if (Contains(message, "dynamic code", "DynamicMethod", "requires dynamic code", "runtime code generation"))
               {
                   score += 4;
                   evidence.Add("Runtime code generation requirement detected.");
               }

               if (Contains(stack, "System.Reflection.Emit", "DynamicMethod", "RuntimeFeature.IsDynamicCodeSupported"))
               {
                   score += 4;
                   evidence.Add("Dynamic code generation API detected in stack trace.");
               }

               if (Contains(type, "PlatformNotSupportedException", "NotSupportedException"))
               {
                   score += 1;
                   evidence.Add("Potential runtime feature incompatibility exception detected.");
               }

               return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
           });
        }

        private static CrashClassificationRule ReflectionRule()
        {
            return new CrashClassificationRule(CrashCategory.Reflection, "Reflection-related failure", "The crash appears to involve runtime reflection.", "A type, constructor, property or method may not be available when accessed dynamically.", "Check reflection targets, dynamically accessed members and Release trimming annotations.", true, (type, message, stack) =>
            {
                var score = 0;
                var evidence = new List<string>();

                if (Contains(type, "MissingMethodException", "MissingFieldException", "TypeLoadException", "TargetInvocationException", "AmbiguousMatchException"))
                {
                    score += 3;
                    evidence.Add("Exception type commonly associated with reflection detected.");
                }

                if (Contains(stack, "System.Reflection", "RuntimeType", "MethodInfo", "PropertyInfo", "ConstructorInfo"))
                {
                    score += 5;
                    evidence.Add("System.Reflection infrastructure detected in stack trace.");
                }

                if (Contains(message, "GetMethod", "GetProperty", "GetField", "GetConstructor", "Activator.CreateInstance", "reflection"))
                {
                    score += 4;
                    evidence.Add("Reflection API or reflection-related message detected.");
                }

                if (Contains(message, "constructor", "member was not found", "method not found", "property not found"))
                {
                    score += 2;
                    evidence.Add("Possible dynamically accessed member missing at runtime.");
                }

                return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
            });
        }

        private static CrashClassificationRule SerializationRule()
        {
            return new CrashClassificationRule(CrashCategory.Serialization, "Serialization failure", "The exception appears to originate from serialization or deserialization.", "The object model, payload or serializer configuration may be incompatible with the runtime data or AOT/trimming requirements.", "Check DTOs, JSON payloads, converters, serializer options and source-generation configuration.", true, (type, message, stack) =>
           {
               var score = 0;
               var evidence = new List<string>();

               if (Contains(type, "JsonException", "SerializationException", "JsonSerializationException", "NotSupportedException"))
               {
                   score += 4;
                   evidence.Add("Serialization-related exception type detected.");
               }

               if (Contains(stack, "System.Text.Json", "Newtonsoft.Json", "JsonSerializer", "JsonConverter"))
               {
                   score += 5;
                   evidence.Add("Serialization framework detected in stack trace.");
               }

               if (Contains(message, "serialize", "serialization", "deserialize", "deserialization", "JSON"))
               {
                   score += 3;
                   evidence.Add("Serialization indicator found in exception message.");
               }

               if (Contains(message, "could not be converted", "invalid JSON", "JSON value", "required property", "constructor parameter"))
               {
                   score += 3;
                   evidence.Add("Possible payload-to-model mapping failure detected.");
               }

               return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
           });
        }

        private static CrashClassificationRule NavigationRule()
        {
            return new CrashClassificationRule(CrashCategory.Navigation, "Navigation-related failure", "The crash appears to involve MAUI application navigation.", "A route, page, navigation parameter or navigation state may be invalid.", "Check Shell route registration, navigation parameters, page creation and lifecycle timing.", false, (type, message, stack) =>
            {
                var score = 0;
                var evidence = new List<string>();

                if (Contains(message, "route not found", "unable to figure out route", "navigation", "Shell route"))
                {
                    score += 4;
                    evidence.Add("Navigation or Shell route failure detected in exception message.");
                }

                if (Contains(stack, "ShellNavigation", "Shell.GoToAsync", "INavigation", "NavigationPage", "Routing"))
                {
                    score += 5;
                    evidence.Add("MAUI navigation infrastructure detected in stack trace.");
                }

                if (Contains(message, "GoToAsync", "PushAsync", "PopAsync", "route"))
                {
                    score += 2;
                    evidence.Add("Navigation API indicator found in exception message.");
                }

                if (Contains(type, "InvalidOperationException", "ArgumentException"))
                {
                    score += 1;
                    evidence.Add("Exception type compatible with invalid navigation state detected.");
                }

                return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
            });
        }

        private static CrashClassificationRule NetworkRule()
        {
            return new CrashClassificationRule(CrashCategory.Network, "Network failure", "The exception appears to originate from network communication.", "The remote endpoint, connection, DNS resolution, TLS negotiation or request timeout may have failed.", "Check connectivity, endpoint availability, DNS, TLS configuration, timeout settings and retry policies.", false, (type, message, stack) =>
           {
               var score = 0;
               var evidence = new List<string>();

               if (Contains(type, "HttpRequestException", "SocketException", "WebException"))
               {
                   score += 5;
                   evidence.Add("Network-related exception type detected.");
               }

               if (Contains(stack, "System.Net.Http", "HttpClient", "SocketsHttpHandler", "System.Net.Sockets"))
               {
                   score += 4;
                   evidence.Add("Network communication framework detected in stack trace.");
               }

               if (Contains(message, "connection refused", "connection reset", "network is unreachable", "host is unreachable", "name or service not known", "DNS"))
               {
                   score += 4;
                   evidence.Add("Network connectivity failure detected in exception message.");
               }

               if (Contains(message, "timeout", "timed out", "HTTP", "SSL", "TLS", "certificate"))
               {
                   score += 2;
                   evidence.Add("HTTP, timeout or transport-security indicator detected.");
               }

               return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
           });
        }

        private static CrashClassificationRule DatabaseRule()
        {
            return new CrashClassificationRule(CrashCategory.Database, "Database failure", "The exception appears to originate from database access.", "A connection, query, migration, transaction or database state may be invalid.", "Check database connectivity, schema migrations, queries, transactions and local database state.", false, (type, message, stack) =>
            {
                var score = 0;
                var evidence = new List<string>();

                if (Contains(type, "DbException", "SqlException", "SqliteException", "PostgresException"))
                {
                    score += 5;
                    evidence.Add("Database-related exception type detected.");
                }

                if (Contains(stack, "EntityFrameworkCore", "Microsoft.Data.Sqlite", "SQLite", "Npgsql", "SqlClient", "DbCommand"))
                {
                    score += 5;
                    evidence.Add("Database access framework detected in stack trace.");
                }

                if (Contains(message, "database", "SQL", "constraint failed", "no such table", "no such column", "syntax error"))
                {
                    score += 3;
                    evidence.Add("Database or SQL failure indicator detected in exception message.");
                }

                if (Contains(message, "migration", "transaction", "foreign key", "unique constraint"))
                {
                    score += 2;
                    evidence.Add("Possible schema, transaction or constraint issue detected.");
                }

                return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
            });
        }

        private static CrashClassificationRule DependencyInjectionRule()
        {
            return new CrashClassificationRule(CrashCategory.DependencyInjection, "Dependency injection failure", "The application appears to have failed while resolving or constructing a service.", "A required service may not have been registered, a constructor dependency may be missing, or the dependency graph may be invalid.", "Check IServiceCollection registrations, service lifetimes and constructor dependencies.", true, (type, message, stack) =>
            {
                var score = 0;
                var evidence = new List<string>();

                if (Contains(message, "Unable to resolve service", "No service for type", "has not been registered"))
                {
                    score += 6;
                    evidence.Add("Missing dependency injection registration detected.");
                }

                if (Contains(message, "A circular dependency was detected", "circular dependency"))
                {
                    score += 6;
                    evidence.Add("Circular dependency detected.");
                }

                if (Contains(stack, "Microsoft.Extensions.DependencyInjection", "ServiceProvider", "CallSiteFactory", "ActivatorUtilities"))
                {
                    score += 5;
                    evidence.Add("Microsoft dependency injection infrastructure detected in stack trace.");
                }

                if (Contains(type, "InvalidOperationException"))
                {
                    score += 1;
                    evidence.Add("Exception type commonly used by dependency injection failures detected.");
                }

                if (Contains(message, "constructor", "activation", "construct"))
                {
                    score += 2;
                    evidence.Add("Service activation or constructor failure detected.");
                }

                return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
            });
        }

        private static CrashClassificationRule MemoryRule()
        {
            return new CrashClassificationRule(CrashCategory.Memory, "Memory exhaustion", "The application appears to have exhausted available managed or native memory.", "Large allocations, images, collections, buffers or unmanaged resources may have caused excessive memory pressure.", "Inspect memory usage, large object allocations, image dimensions, cached data and unmanaged resource lifetimes.", false, (type, message, stack) =>
            {
                var score = 0;
                var evidence = new List<string>();

                if (Contains(type, "OutOfMemoryException"))
                {
                    score += 10;
                    evidence.Add("OutOfMemoryException detected.");
                }

                if (Contains(message, "out of memory", "cannot allocate memory", "failed to allocate"))
                {
                    score += 6;
                    evidence.Add("Memory allocation failure detected in exception message.");
                }

                if (Contains(stack, "Bitmap", "SKBitmap", "ImageSource", "MemoryStream", "Array.Resize"))
                {
                    score += 2;
                    evidence.Add("Potential memory-intensive operation detected in stack trace.");
                }

                return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
            });
        }

        private static CrashClassificationRule ThreadingRule()
        {
            return new CrashClassificationRule(CrashCategory.Threading, "Threading/concurrency failure", "The exception appears to involve thread affinity, concurrent execution or synchronization.", "A UI-thread violation, race condition, concurrent collection modification or invalid synchronization state may be involved.", "Check MainThread usage, Dispatcher calls, synchronization primitives and shared mutable state.", false, (type, message, stack) =>
            {
                var score = 0;
                var evidence = new List<string>();

                if (Contains(message, "main thread", "UI thread", "thread that created it", "different thread", "wrong thread"))
                {
                    score += 6;
                    evidence.Add("UI-thread affinity violation detected.");
                }

                if (Contains(message, "collection was modified", "concurrent", "race condition", "synchronization"))
                {
                    score += 4;
                    evidence.Add("Concurrency or synchronization issue detected.");
                }

                if (Contains(stack, "MainThread", "Dispatcher", "SynchronizationContext", "Monitor.Enter", "SemaphoreSlim", "Interlocked"))
                {
                    score += 3;
                    evidence.Add("Threading or synchronization infrastructure detected in stack trace.");
                }

                if (Contains(type, "SynchronizationLockException", "ThreadStateException"))
                {
                    score += 5;
                    evidence.Add("Threading-specific exception type detected.");
                }

                if (Contains(type, "InvalidOperationException") && Contains(message, "thread", "concurrent", "collection"))
                {
                    score += 3;
                    evidence.Add("InvalidOperationException associated with threading state detected.");
                }

                return score == 0 ? null : new CrashRuleMatch
                {
                    Score = score,
                    Evidence = evidence
                };
            });
        }

        //private static bool ContainsAny(
        //    string? value,
        //    params string[] terms)
        //{
        //    if (string.IsNullOrWhiteSpace(value))
        //        return false;

        //    foreach (var term in terms)
        //    {
        //        if (value.Contains(
        //                term,
        //                StringComparison.OrdinalIgnoreCase))
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        //private static IReadOnlyList<string> BuildEvidence(
        //    string type,
        //    string message,
        //    string? stack)
        //{
        //    var evidence = new List<string>();

        //    if (!string.IsNullOrWhiteSpace(type))
        //    {
        //        evidence.Add(
        //            $"Exception type: {type}");
        //    }

        //    if (!string.IsNullOrWhiteSpace(message))
        //    {
        //        evidence.Add(
        //            $"Message contains diagnostic indicators.");
        //    }

        //    if (!string.IsNullOrWhiteSpace(stack))
        //    {
        //        evidence.Add(
        //            "Stack trace contains diagnostic indicators.");
        //    }

        //    return evidence;
        //}

        private static bool Contains(string? value, params string[] terms)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            foreach (var term in terms)
            {
                if (value.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static IReadOnlyList<string> BuildBasicEvidence(string type, string message, string? stack)
        {
            var evidence = new List<string>();

            if (!string.IsNullOrWhiteSpace(type))
            {
                evidence.Add($"Exception type: {type}");
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                evidence.Add("Exception message available.");
            }

            if (!string.IsNullOrWhiteSpace(stack))
            {
                evidence.Add("Stack trace available.");
            }

            return evidence;
        }
    }
}
