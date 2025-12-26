using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Tipping
{
    public static class DeadlineService
    {
        public static readonly TimeSpan CACHE_LENGTH = new(days: 1, 0, 0, 0);

        private static readonly CachedDeadline _deadline = new(DateTimeOffset.MinValue, null);
        private static readonly SemaphoreSlim settingsSemaphore = new(1, 1);

        public static async Task<DateTimeOffset> GetNextDeadlineAsync(ModelDbContext modelDb)
        {
            await settingsSemaphore.WaitAsync();
            try
            {
                if (_deadline.Expiry > DateTimeOffset.UtcNow
                    && _deadline.Value is not null
                    && _deadline.Value > DateTimeOffset.UtcNow)
                {
                    return _deadline.Value.Value;
                }

                _deadline.Value = await GetNextDeadlineImpl(modelDb);
                _deadline.Expiry = DateTimeOffset.UtcNow + CACHE_LENGTH;
                return _deadline.Value.Value;
            }
            finally
            {
                settingsSemaphore.Release();
            }
        }

        public static void ExpireSettings()
        {
            _deadline.Expiry = DateTimeOffset.MinValue;
        }

        protected class CachedDeadline(DateTimeOffset expiry, DateTimeOffset? value)
        {
            public DateTimeOffset Expiry { get; set; } = expiry;
            public DateTimeOffset? Value { get; set; } = value;
        }

        private static async Task<DateTimeOffset> GetNextDeadlineImpl(ModelDbContext modelDb)
        {
            var utcNow = DateTimeOffset.UtcNow;
            return (await modelDb.Events.ToArrayAsync())
                .Where(e => e.TipsDeadline > utcNow)
                .OrderBy(e => e.TipsDeadline)
                .FirstOrDefault()?.TipsDeadline ?? DateTimeOffset.MaxValue;
        }
    }
}
