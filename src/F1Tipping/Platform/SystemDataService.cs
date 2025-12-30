using F1Tipping.Data;

namespace F1Tipping.Platform;

public static class SystemDataService
{
    public static readonly TimeSpan CACHE_LENGTH = new TimeSpan(0, minutes: 30, 0);

    private static CachedSettings _settings = new(DateTimeOffset.MinValue, null);
    private static readonly SemaphoreSlim settingsSemaphore = new(1, 1);

    public static async Task<SystemSettings> GetSystemSettingsAsync(AppDbContext appDb)
    {
        await settingsSemaphore.WaitAsync();
        try
        {
            if (_settings.Expiry > DateTimeOffset.UtcNow
                && _settings.Value is not null)
            {
                return _settings.Value;
            }

            _settings.Value = await appDb.GetSystemSettingsAsync();
            _settings.Expiry = DateTimeOffset.UtcNow + CACHE_LENGTH;
            return _settings.Value;
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

    protected class CachedSettings(DateTimeOffset expiry, SystemSettings? value)
    {
        public DateTimeOffset Expiry { get; set; } = expiry;
        public SystemSettings? Value { get; set; } = value;
    }
}
