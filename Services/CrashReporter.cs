using AkengMauiCrashReporter.Configuration;
using AkengMauiCrashReporter.Environments;
using AkengMauiCrashReporter.Formatting;
using AkengMauiCrashReporter.Internal;
using AkengMauiCrashReporter.Lifecycle;
using AkengMauiCrashReporter.Models;
using AkengMauiCrashReporter.Models.Enums;
using AkengMauiCrashReporter.Storage;

namespace AkengMauiCrashReporter.Services
{
    internal sealed class CrashReporter : ICrashReporter
    {
        private readonly IPendingCrashStore _pendingCrashStore;
        private readonly ICrashReportStore _reportStore;

        private readonly CrashLoopDetector _crashLoopDetector;
        private readonly EnvironmentSnapshotProvider _environmentSnapshotProvider;
        private readonly BreadcrumbStore _breadcrumbStore;
        private readonly MauiCrashReporterOptions _options;
        //private readonly CrashExceptionMapper _exceptionMapper;
        //private readonly CrashFingerprintGenerator _fingerprintGenerator;
        private readonly CrashSessionManager _sessionManager;
        private readonly CrashReportFactory _reportFactory;
        private readonly object _syncRoot = new();
        private readonly Dictionary<string, string> _context = [];
        private readonly CrashReportFormatter _reportFormatter;
        private readonly EmergencyCrashWriter _emergencyCrashWriter;

        //private EnvironmentSnapshot? _environmentSnapshot;
        //private MauiLifecycleTracker? _lifecycleTracker;
        private int _initializationStarted;
        private int _initialized;
        private readonly TaskCompletionSource _initializationCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal Task InitializationCompleted =>
            _initializationCompletion.Task;
        public bool IsInitialized => Volatile.Read(ref _initialized) == 1;
        public bool HasPendingCrash => _pendingCrashStore.HasPendingCrash;
        public Guid SessionId => _sessionManager.SessionId;

        public CrashReporter(MauiCrashReporterOptions options, CrashReportFactory reportFactory, CrashSessionManager sessionManager, EmergencyCrashWriter emergencyCrashWriter, IPendingCrashStore pendingCrashStore, ICrashReportStore reportStore, CrashLoopDetector crashLoopDetector, EnvironmentSnapshotProvider environmentSnapshotProvider, BreadcrumbStore breadcrumbStore, /*MauiLifecycleTracker lifecycleTracker,*/ CrashReportFormatter reportFormatter)
        {
            ArgumentNullException.ThrowIfNull(options);

            options.Validate();

            _options = options;

            //_exceptionMapper = new CrashExceptionMapper(options);

            //_fingerprintGenerator = new CrashFingerprintGenerator();

            _sessionManager = sessionManager;

            _reportFactory = reportFactory;

            _emergencyCrashWriter = emergencyCrashWriter;

            _pendingCrashStore = pendingCrashStore;

            _crashLoopDetector = crashLoopDetector;

            _environmentSnapshotProvider = environmentSnapshotProvider;

            _breadcrumbStore = breadcrumbStore;

            //_lifecycleTracker = lifecycleTracker;

            _reportFormatter = reportFormatter;

            _reportStore = reportStore;
        }

        public async Task<CrashLoopInfo> DetectCrashLoopAsync(CancellationToken cancellationToken = default)
        {
            var storedReports = await _reportStore.GetPendingAsync(cancellationToken);

            var reports = storedReports.Select(x => x.Report).ToArray();

            return _crashLoopDetector.Analyze(reports);
        }

        internal void StartInitialization()
        {
            if (Interlocked.Exchange(ref _initializationStarted, 1) == 1)
            {
                return;
            }

            /*
             * Run synchronous operations that are cheap
             * and required early.
             */

            try
            {
                _emergencyCrashWriter.CleanupIncompleteWrites();
            }
            catch
            {
            }

            /*
             * Do not block MAUI startup waiting for disk IO.
             */
            _ = InitializeCoreAsync();
        }

        private async Task InitializeCoreAsync()
        {
            try
            {
                await ProcessPendingCrashesAsync(CancellationToken.None);

                if (_options.EnableDiagnostics)
                {
                    await DetectAndHandleCrashLoopAsync(CancellationToken.None);
                }
            }
            catch
            {
                /*
                 * Never propagate package initialization
                 * errors to host application.
                 */
            }
            finally
            {
                Volatile.Write(ref _initialized, 1);
                _initializationCompletion.TrySetResult();
            }
        }

        //public async Task InitializeAsync(CancellationToken cancellationToken = default)
        //{
        //    if (Interlocked.Exchange(ref _initialized, 1) == 1)
        //    {
        //        return;
        //    }

        //    _lifecycleTracker.Track(MauiLifecycleEventType.ApplicationStarted);

        //    //_environmentSnapshot = _environmentSnapshotProvider.Capture();

        //    _emergencyCrashWriter.CleanupIncompleteWrites();

        //    await ProcessPendingCrashesAsync(cancellationToken);

        //    if (_options.EnableDiagnostics)
        //    {
        //        await DetectAndHandleCrashLoopAsync(cancellationToken);
        //    }

        //    //if (_options.CaptureUnhandledExceptions || _options.CaptureUnobservedTaskExceptions)
        //    //{
        //    //    _globalExceptionHandler.Start(_options.CaptureUnhandledExceptions, _options.CaptureUnobservedTaskExceptions);
        //    //}
        //}

        private async Task DetectAndHandleCrashLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var storedReports = await _reportStore.GetPendingAsync(cancellationToken);

                var reports = storedReports.Select(x => x.Report).ToArray();

                var result = _crashLoopDetector.Analyze(reports);

                if (!result.IsDetected)
                    return;

                AddBreadcrumb($"Crash loop detected: {result.Fingerprint}", "CrashLoop");

                SetContext("CrashLoopDetected", "true");

                SetContext("CrashLoopFingerprint", result.Fingerprint ?? "unknown");

                SetContext("CrashLoopOccurrences", result.Occurrences.ToString());

                SetContext("CrashLoopStartup", result.IsStartupCrash.ToString());
            }
            catch
            {
            }
        }

        public void AddBreadcrumb(string message, string? category = null)
        {
            if (!_options.EnableBreadcrumbs)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                _breadcrumbStore.Add(new CrashBreadcrumb
                {
                    TimestampUtc = DateTimeOffset.UtcNow,
                    Message = Truncate(message, _options.MaxBreadcrumbMessageLength),
                    Category = Truncate(category, 128)
                });
            }
            catch
            {
            }
        }

        public void SetContext(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (value is null)
                return;

            try
            {
                var normalizedKey = Truncate(key.Trim(), _options.MaxContextKeyLength);

                var normalizedValue = Truncate(value, _options.MaxContextValueLength);

                lock (_syncRoot)
                {
                    if (!_context.ContainsKey(normalizedKey!))
                    {
                        if (_context.Count >= _options.MaxContextEntries)
                        {
                            return;
                        }
                    }

                    _context[normalizedKey!] = normalizedValue ?? string.Empty;
                }
            }
            catch
            {
            }
        }

        public void RemoveContext(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            try
            {
                lock (_syncRoot)
                {
                    _context.Remove(key);
                }
            }
            catch
            {
            }
        }

        public void ClearContext()
        {
            try
            {
                lock (_syncRoot)
                {
                    _context.Clear();
                }
            }
            catch
            {
            }
        }

        public void ClearBreadcrumbs()
        {
            try
            {
                lock (_syncRoot)
                {
                    _breadcrumbStore.Clear();
                }
            }
            catch
            {
            }
        }

        public async Task<CrashReport?> CaptureExceptionAsync(Exception exception, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(exception);

            var breadcrumbs = GetBreadcrumbSnapshot();

            var context = GetContextSnapshot();

            try
            {
                var report = _reportFactory.Create(exception, CrashSeverity.NonFatal, CrashOrigin.Manual, null, breadcrumbs, context);

                await _reportStore.SaveAsync(report, cancellationToken);

                return report;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }

        public async Task<CrashReport?> CaptureExceptionAsync(Exception exception, CrashOrigin origin, CrashSeverity severity = CrashSeverity.NonFatal, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(exception);

            var breadcrumbs = GetBreadcrumbSnapshot();

            var context = GetContextSnapshot();

            try
            {
                var report = _reportFactory.Create(exception, severity, origin, null, breadcrumbs, context);

                await _reportStore.SaveAsync(report, cancellationToken);

                return report;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }

        public void CaptureFatalException(Exception exception, CrashOrigin origin)
        {
            ArgumentNullException.ThrowIfNull(exception);

            try
            {
                _emergencyCrashWriter.Write(exception, origin, _sessionManager.GetSessionInfo());
            }
            catch
            {
            }
        }

        public async Task<IReadOnlyList<CrashReport>> GetCrashesAsync(CancellationToken cancellationToken = default)
        {
            var storedReports = await _reportStore.GetPendingAsync(cancellationToken);

            var reports = storedReports.Select(x => x.Report).ToArray();

            return reports;
        }

        public async Task DeleteCrashAsync(Guid reportId, CancellationToken cancellationToken = default)
        {
            await _reportStore.DeleteAsync(reportId, cancellationToken);
        }

        public async Task ClearCrashesAsync(CancellationToken cancellationToken = default)
        {
            await _reportStore.ClearAsync(cancellationToken);
        }

        private async Task ProcessPendingCrashesAsync(CancellationToken cancellationToken)
        {
            var pendingReports = await _pendingCrashStore.GetAllAsync(cancellationToken);

            if (pendingReports.Count == 0)
                return;

            foreach (var pending in pendingReports)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var report = _reportFactory.Create(pending);

                    await _reportStore.SaveAsync(report, cancellationToken);

                    await _pendingCrashStore.DeleteAsync(pending.Id, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    // Keep the pending crash if processing fails.
                }
            }
        }

        private IReadOnlyList<CrashBreadcrumb> GetBreadcrumbSnapshot()
        {
            lock (_syncRoot)
            {
                return _breadcrumbStore.Snapshot();
            }
        }

        private IReadOnlyDictionary<string, string> GetContextSnapshot()
        {
            lock (_syncRoot)
            {
                return new Dictionary<string, string>(_context);
            }
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxLength)
                return value;

            const string suffix = "...[truncated]";

            var availableLength = Math.Max(0, maxLength - suffix.Length);

            return string.Concat(value.AsSpan(0, availableLength), suffix);
        }

        public Task<IReadOnlyList<StoredCrashReport>> GetPendingReportsAsync(CancellationToken cancellationToken = default)
        {
            return _reportStore.GetPendingAsync(cancellationToken);
        }

        public Task<StoredCrashReport?> GetLatestReportAsync(CancellationToken cancellationToken = default)
        {
            return _reportStore.GetLatestAsync(cancellationToken);
        }

        public Task<StoredCrashReport?> GetReportAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _reportStore.GetAsync(id, cancellationToken);
        }

        public Task MarkReportAsProcessedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _reportStore.MarkAsProcessedAsync(id, cancellationToken);
        }

        public Task DeleteReportAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _reportStore.DeleteAsync(id, cancellationToken);
        }

        public Task ClearReportsAsync(CancellationToken cancellationToken = default)
        {
            return _reportStore.ClearAsync(cancellationToken);
        }

        public Task<int> GetReportCountAsync(CancellationToken cancellationToken = default)
        {
            return _reportStore.CountAsync(cancellationToken);
        }

        public string FormatReport(CrashReport report, CrashReportFormat format = CrashReportFormat.Markdown)
        {
            ArgumentNullException.ThrowIfNull(report);

            return _reportFormatter.Format(report, format);
        }
    }
}
