using Microsoft.AspNetCore.Mvc;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Tipping;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Tipping
{
    public class FullScoresModel : PlayerPageModel
    {
        private TipScoringService _tipScoring;

        public FullScoresModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb,
            TipScoringService tipScoring)
            : base(userManager, appDb, modelDb)
        {
            _tipScoring = tipScoring;
        }

        [BindProperty]
        public Dictionary<(Guid Player, Guid Event), TipScoringService.PlayerEventReport> Reports { get; set; } = new();
        [BindProperty]
        public List<PlayerView> Players { get; set; } = new();
        [BindProperty]
        public List<EventView> Events { get; set; } = new();

        public async Task<IActionResult> OnGet()
        {
            var playerList = (await _modelDb.Players.ToListAsync())
                .OrderBy(p => p == Player)
                .ThenBy(p => p.Details?.DisplayName ?? p.Details?.FirstName)
                .ToList();
            // TODO: Rework this after changing how TipsDeadline works
            var eventList = (await _modelDb.Events.ToListAsync())
                .Where(e => e.TipsDeadline < DateTimeOffset.UtcNow)
                .OrderBy(e => e.OrderKey);
            var resultsList = (await (
                from r in _modelDb.Results
                join eId in eventList.Select(e => e.Id) on r.Event.Id equals eId
                select r).ToListAsync()).ToLookup(r => r.Event.Id);
            var tipsList =
                (await (
                    from t in _modelDb.Tips
                    join eId in eventList.Select(e => e.Id) on t.Target.Event.Id equals eId
                    group t by new { eId, t.Tipper.Id, t.Target.Type } into tgroup
                    //select tgroup.MaxBy(t => t.SubmittedAt) -- MaxBy is too new for efcore?!
                    select tgroup.OrderByDescending(t => t.SubmittedAt).First())
                .ToListAsync())
                .ToLookup(t => (t.Tipper.Id, t.Target.Event.Id));

            foreach (var p in playerList)
            {
                Players.Add(new(p.Id, p.Details?.DisplayName
                    ?? p.Details?.FirstName ?? "Unknown Player!"));
            }

            foreach (var e in eventList)
            {
                var results = resultsList[e.Id].OrderBy(r => r.Type).ToList();
                Events.Add(new(e.Id, e.EventName, results));

                foreach (var p in playerList)
                {
                    var tips = tipsList[(p.Id, e.Id)].ToList();
                    var report = _tipScoring.GetReport(e, tips);
                    if (report is not null && report.ScoredTips.Count > 0)
                    {
                        Reports[(p.Id, e.Id)] = report;
                    }
                }
            }

            return Page();
        }

        public record PlayerView(Guid Id, string Name);
        public record EventView(Guid Id, string Name, List<Result> Results);
    }
}
