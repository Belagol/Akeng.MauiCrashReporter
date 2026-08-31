using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Services;

namespace AkengMauiCrashReporter.Lifecycle
{
    public sealed class MauiLifecycleTracker
    {
        private readonly BreadcrumbStore _breadcrumbStore;

        public MauiLifecycleTracker(BreadcrumbStore breadcrumbStore)
        {
            _breadcrumbStore = breadcrumbStore;
        }

        public void Track(MauiLifecycleEventType type, object? source = null, string? route = null, string? details = null)
        {
            var objectType = source?.GetType().FullName;

            var objectName = source?.GetType().Name;

            var message = BuildMessage(type, objectName, route);

            _breadcrumbStore.Add(new CrashBreadcrumb
            {
                TimestampUtc = DateTime.UtcNow,
                Category = "Lifecycle",
                Message = message,
                Data = BuildData(type, objectType, route, details)
            });
        }

        private static string BuildMessage(MauiLifecycleEventType type, string? objectName, string? route)
        {
            return type switch
            {
                MauiLifecycleEventType.ApplicationStarted => "Application started",

                MauiLifecycleEventType.ApplicationPaused => "Application paused",

                MauiLifecycleEventType.ApplicationStopping => "Application stopping",

                MauiLifecycleEventType.ApplicationResumed => "Application resumed",

                MauiLifecycleEventType.WindowCreated => $"Window created: {objectName}",

                MauiLifecycleEventType.WindowDestroyed => $"Window destroyed: {objectName}",

                MauiLifecycleEventType.PageCreated => $"Page created: {objectName}",

                MauiLifecycleEventType.PageAppearing => $"Page appearing: {objectName}",

                MauiLifecycleEventType.PageDisappearing => $"Page disappearing: {objectName}",

                MauiLifecycleEventType.PageDestroyed => $"Page destroyed: {objectName}",

                MauiLifecycleEventType.NavigationStarted => $"Navigation started: {route}",

                MauiLifecycleEventType.NavigationCompleted => $"Navigation completed: {route}",

                MauiLifecycleEventType.HandlerCreated => $"Handler created: {objectName}",

                MauiLifecycleEventType.HandlerDisconnected => $"Handler disconnected: {objectName}",

                _ => type.ToString()
            };
        }

        private static IReadOnlyDictionary<string, string> BuildData(MauiLifecycleEventType type, string? objectType, string? route, string? details)
        {
            var data = new Dictionary<string, string>
            {
                ["Event"] = type.ToString()
            };

            if (!string.IsNullOrWhiteSpace(objectType))
            {
                data["ObjectType"] = objectType;
            }

            if (!string.IsNullOrWhiteSpace(route))
            {
                data["Route"] = route;
            }

            if (!string.IsNullOrWhiteSpace(details))
            {
                data["Details"] = details;
            }

            return data;
        }
    }
}
