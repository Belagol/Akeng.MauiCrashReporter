using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Models;

namespace AkengMauiCrashReporter.Internal
{
    internal sealed class CrashExceptionMapper
    {
        private readonly MauiCrashReporterOptions _options;

        public CrashExceptionMapper(MauiCrashReporterOptions options)
        {
            _options = options;
        }

        public CrashExceptionInfo Map(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return MapInternal(exception, depth: 0);
        }

        private CrashExceptionInfo MapInternal(Exception exception, int depth)
        {
            var exceptionType = exception.GetType().FullName ?? exception.GetType().Name;

            if (depth >= _options.MaxExceptionDepth)
            {
                return new CrashExceptionInfo
                {
                    Type = exceptionType,
                    Message = "[Maximum inner exception depth reached]",
                    HResult = exception.HResult
                };
            }

            var innerExceptions = exception is AggregateException aggregateException ? MapAggregateExceptions(aggregateException, depth) : [];

            return new CrashExceptionInfo
            {
                Type = exceptionType,
                Message = Truncate(exception.Message, _options.MaxExceptionMessageLength) ?? string.Empty,
                StackTrace = Truncate(exception.StackTrace, _options.MaxStackTraceLength),
                Source = Truncate(exception.Source, 512),
                HResult = exception.HResult,
                InnerException = exception is AggregateException ? null : exception.InnerException is null
                        ? null : MapInternal(exception.InnerException, depth + 1),
                InnerExceptions = innerExceptions
            };
        }

        private IReadOnlyList<CrashExceptionInfo> MapAggregateExceptions(AggregateException aggregateException, int depth)
        {
            if (depth >= _options.MaxExceptionDepth)
                return [];

            try
            {
                var flattened = aggregateException.Flatten();

                return flattened.InnerExceptions.Take(_options.MaxAggregateExceptions)
                    .Select(exception => MapInternal(exception, depth + 1))
                    .ToArray();
            }
            catch
            {
                return [];
            }
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            const string suffix = "\n...[truncated by Akeng.MauiCrashReporter]";

            var availableLength = Math.Max(0, maxLength - suffix.Length);

            return string.Concat(value.AsSpan(0, availableLength), suffix);
        }
    }
}
