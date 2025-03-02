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

            var eventScoreMult = eventWithResults switch
            {
                Race r => RaceTypeHelper.GetAttributes<ScoreMultAttribute>(r.Type)
                            .First().Mult,
                Season s => 1.0m,
                _ => throw new NotImplementedException(),
            };
            var scoredTips = new List<ScoredTip>();
            var playerTips = await _tipService.GetTipsAsync(player, eventWithResults);
            var tipMap = playerTips.ToDictionary(tip => tip.Target.Type);
            foreach (var tip in playerTips)
            {
                var scoredTip = new ScoredTip() { Tip = tip };
                scoredTips.Add(scoredTip);
                var scorers = ResultTypeHelper
                    .GetAttributes<ScoresAttribute>(tip.Target.Type);

                foreach (var scorer in scorers)
                {
                    if (tip.Target.EntityInResult(tip.Selection))
                    {
                        scoredTip.Score += scorer.MatchPoints * eventScoreMult;
                    }

                    foreach (var altType in scorer.AlternateResults)
                    {
                        if (tipMap[altType].Target.EntityInResult(tip.Selection))
                        {
                            scoredTip.Score += scorer.AlternatePoints * eventScoreMult;
                        }
                    }
                }
            }

            return new PlayerEventReport() { ScoredTips = scoredTips };
        }

        public class PlayerEventReport
        {
            public List<ScoredTip> ScoredTips { get; set; } = new();
            public decimal? EventScore { get => ScoredTips.Any()
                    ? ScoredTips.Select(x => x.Score).Aggregate((x, y) => x + y)
                    : null; }
        }

        public class ScoredTip
        {
            public decimal Score { get; set; } = 0.0m;
            public required Tip Tip { get; set; }
        }
    }
}