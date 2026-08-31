using AkengMauiCrashReporter.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AkengMauiCrashReporter.Internal
{
    internal sealed class CrashFingerprintGenerator
    {
        private static readonly Regex GuidRegex = new(@"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b", RegexOptions.Compiled);

        private static readonly Regex HexAddressRegex = new(@"0x[0-9a-fA-F]+", RegexOptions.Compiled);

        private static readonly Regex LongNumberRegex = new(@"\b\d{5,}\b", RegexOptions.Compiled);

        private const int FingerprintLength = 16;

        public string Generate(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            try
            {
                var input = BuildFingerprintInput(exception);

                var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

                return Convert.ToHexString(bytes).Substring(0, FingerprintLength);
            }
            catch
            {
                return GenerateFallback(exception);
            }
        }

        public string Generate(CrashExceptionInfo exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            try
            {
                var type = exception.Type;

                var message = NormalizeMessage(exception.Message);

                var stack = NormalizeStackTrace(exception.StackTrace);

                var input = string.Join("|", type, message, stack);

                var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

                return Convert.ToHexString(bytes).Substring(0, FingerprintLength);
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        private static string BuildFingerprintInput(Exception exception)
        {
            var type = exception.GetType().FullName ?? exception.GetType().Name;

            var message = NormalizeMessage(exception.Message);

            var stack = NormalizeStackTrace(exception.StackTrace);

            return string.Join("|", type, message, stack);
        }

        private static string NormalizeMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            var normalized = message.Trim().Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Replace('\r', '\n');

            return NormalizeDynamicValues(normalized);
        }

        private static string NormalizeStackTrace(string? stackTrace)
        {
            if (string.IsNullOrWhiteSpace(stackTrace))
                return string.Empty;

            var lines = stackTrace.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Take(8)
                    .Select(NormalizeStackFrame);

            return string.Join("\n", lines);
        }

        private static string NormalizeStackFrame(string frame)
        {
            var normalized = frame.Trim();

            var lineNumberIndex = normalized.LastIndexOf(":line ", StringComparison.OrdinalIgnoreCase);

            if (lineNumberIndex >= 0)
            {
                normalized = normalized[..lineNumberIndex];
            }

            return normalized;
        }       

        private static string NormalizeDynamicValues(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value;

            normalized = GuidRegex.Replace(normalized, "{guid}");

            normalized = HexAddressRegex.Replace(normalized, "{hex}");

            normalized = LongNumberRegex.Replace(normalized, "{number}");

            return normalized;
        }

        private static string GenerateFallback(Exception exception)
        {
            try
            {
                var value = exception.GetType().FullName ?? exception.GetType().Name;

                var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));

                return Convert.ToHexString(bytes).Substring(0, FingerprintLength);
            }
            catch
            {
                return "UNKNOWN";
            }
        }
    }
}
