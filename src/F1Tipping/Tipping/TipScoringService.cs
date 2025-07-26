using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace F1Tipping.Tipping
{
    public class TipScoringService
    {
        private ModelDbContext _modelDb;

        public TipScoringService(ModelDbContext modelDb)
        {
            _modelDb = modelDb;
        }

        public async Task<(decimal completed, decimal provisional)> GetPlayerScoreAsync(Player player)
        {
            var completeEvents = await _modelDb.Events.Where(e => e.Completed).ToListAsync();
            var completedScore = 0m;
            foreach (var e in completeEvents)
            {
                completedScore += (await GetReportAsync(player, e))?.EventScore ?? 0m;
            }

            var incompleteEvents = await _modelDb.Events.Where(e => !e.Completed).ToListAsync();
            var provisionalScore = 0m;
            foreach (var e in incompleteEvents)
            {
                provisionalScore += (await GetReportAsync(player, e))?.EventScore ?? 0m;
            }

            return (completedScore, provisionalScore);
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
            var playerTips = await TipReportingService.GetTipsAsync(player, eventWithResults, _modelDb);
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

            return new PlayerEventReport() { ScoredTips = scoredTips.ToDictionary(st => st.Tip.Target.Type) };
        }

        public PlayerEventReport? GetReport(Event @event, IList<Tip> playerTips)
        {
            if (playerTips.IsNullOrEmpty())
            {
                return null;
            }

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

            return new PlayerEventReport() { ScoredTips = scoredTips.ToDictionary(st => st.Tip.Target.Type) };
        }

        public class PlayerEventReport
        {
            public Dictionary<ResultType,ScoredTip> ScoredTips { get; set; } = new();
            public decimal? EventScore { get => ScoredTips.Any()
                    ? ScoredTips.Values.Select(x => x.Score).Aggregate((x, y) => x + y)
                    : null; }
        }

        public class ScoredTip
        {
            public decimal Score { get; set; } = 0.0m;
            public required Tip Tip { get; set; }
        }
    }
}