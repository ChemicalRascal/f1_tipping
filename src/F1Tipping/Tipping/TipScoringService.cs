using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;

namespace F1Tipping.Tipping
{
    public class TipScoringService
    {
        private ModelDbContext _modelDb;
        private TipReportingService _tipService;

        public TipScoringService(ModelDbContext modelDb, TipReportingService tipService)
        {
            _modelDb = modelDb;
            _tipService = tipService;
        }

        public async Task<PlayerEventReport?> GetReportAsync(Player player, Event @event)
        {
            if (@event is not IEventWithResults eventWithResults)
            {
                return null;
            }
            var playerTips = await _tipService.GetTipsAsync(player, eventWithResults);
            return null;
        }

        public class PlayerEventReport
        {
            public List<ScoredTip> ScoredTips { get; set; } = new();
            public int EventScore { get => ScoredTips
                    .Select(x => x.Score)
                    .Aggregate((x, y) => x + y); }
        }

        public class ScoredTip : Tip
        {
            public int Score { get; set; }
        }
    }
}