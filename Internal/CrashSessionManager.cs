using AkengMauiCrashReporter.Models;

namespace AkengMauiCrashReporter.Internal
{
    internal sealed class CrashSessionManager
    {
        private readonly object _syncRoot = new();

        private Guid _sessionId;
        private DateTimeOffset _startedAtUtc;

        public CrashSessionManager()
        {
            StartNewSession();
        }

        public Guid SessionId
        {
            get
            {
                lock (_syncRoot)
                {
                    return _sessionId;
                }
            }
        }

        public DateTimeOffset StartedAtUtc
        {
            get
            {
                lock (_syncRoot)
                {
                    return _startedAtUtc;
                }
            }
        }

        public TimeSpan Duration
        {
            get
            {
                lock (_syncRoot)
                {
                    return DateTimeOffset.UtcNow - _startedAtUtc;
                }
            }
        }

        public void StartNewSession()
        {
            lock (_syncRoot)
            {
                _sessionId = Guid.NewGuid();
                _startedAtUtc = DateTimeOffset.UtcNow;
            }
        }

        public CrashSessionInfo GetSessionInfo()
        {
            lock (_syncRoot)
            {
                return new CrashSessionInfo
                {
                    SessionId = _sessionId,
                    StartedAtUtc = _startedAtUtc,
                    Duration = DateTimeOffset.UtcNow - _startedAtUtc
                };
            }
        }
    }
}
