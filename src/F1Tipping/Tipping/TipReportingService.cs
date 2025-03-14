using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Tipping
{
    public static class TipReportingService
    {
        public static readonly TimeSpan CACHE_LENGTH = new TimeSpan(0, minutes: 30, 0);

        private static readonly Dictionary<(Guid PlayerId, Guid EventId), CachedTips> _tipsCache = [];

        public static async Task<IEnumerable<Tip>> GetTipsAsync(Player player, IEventWithResults @event, ModelDbContext modelDb)
        {
            if (_tipsCache.TryGetValue((player.Id, ((Event)@event).Id), out var tips)
                && tips.Expiry > DateTimeOffset.UtcNow)
            {
                return tips.Value ?? Array.Empty<Tip>();
            }

            tips = new CachedTips(DateTimeOffset.UtcNow + CACHE_LENGTH,
                await modelDb.Tips
                    .Where(t => t.Target.Event == @event && t.Tipper == player)
                    .GroupBy(t => t.Target.Type)
                    .Select(tg => tg.OrderByDescending(t => t.SubmittedAt).First())
                    .ToListAsync());
            _tipsCache[(player.Id, ((Event)@event).Id)] = tips;

            return tips.Value ?? Array.Empty<Tip>();
        }

        public static void BustCache(Player player, Event @event)
        {
            if (_tipsCache.ContainsKey((player.Id, @event.Id)))
            {
                _tipsCache.Remove((player.Id, @event.Id));
            }
        }

        private class CachedTips(DateTimeOffset expiry, IList<Tip> value)
        {
            public DateTimeOffset Expiry { get; set; } = expiry;
            public IList<Tip>? Value { get; set; } = value;
        }
    }
}