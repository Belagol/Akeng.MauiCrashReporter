using AkengMauiCrashReporter.Models;

namespace AkengMauiCrashReporter.Services
{
    public sealed class BreadcrumbStore
    {
        private readonly object _sync = new();

        private readonly Queue<CrashBreadcrumb> _items = new();

        private readonly int _capacity;

        public BreadcrumbStore(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
        }

        public void Add(CrashBreadcrumb breadcrumb)
        {
            ArgumentNullException.ThrowIfNull(breadcrumb);

            if (_capacity == 0)
                return;

            lock (_sync)
            {
                _items.Enqueue(breadcrumb);

                while (_items.Count > _capacity)
                {
                    _items.Dequeue();
                }
            }
        }

        public IReadOnlyList<CrashBreadcrumb> Snapshot()
        {
            lock (_sync)
            {
                return _items.ToArray();
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _items.Clear();
            }
        }
    }
}
