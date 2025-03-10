using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Tipping
{
    public static class DeadlineService
    {
        public static readonly TimeSpan CACHE_LENGTH = new TimeSpan(0, minutes: 30, 0);

        private static CachedDeadline _settings = new(DateTimeOffset.MinValue, null);
        private static readonly SemaphoreSlim settingsSemaphore = new(1, 1);

        public static async Task<DateTimeOffset> GetNextDeadlineAsync(ModelDbContext modelDb)
        {
            await settingsSemaphore.WaitAsync();
            try
            {
                if (_settings.Expiry > DateTimeOffset.UtcNow
                    && _settings.Value is not null)
                {
                    return _settings.Value.Value;
                }

                _settings.Value = await GetNextDeadlineImpl(modelDb);
                _settings.Expiry = DateTimeOffset.UtcNow + CACHE_LENGTH;
                return _settings.Value.Value;
            }
            finally
            {
                settingsSemaphore.Release();
            }
        }

        public static void ExpireSettings()
        {
            _settings.Expiry = DateTimeOffset.MinValue;
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
