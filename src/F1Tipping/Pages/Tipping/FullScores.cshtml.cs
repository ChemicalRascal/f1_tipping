using Microsoft.AspNetCore.Mvc;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using F1Tipping.Tipping;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Tipping
{
    public class FullScoresModel : PlayerPageModel
    {
        private const int DISPLAY_NAME_LEN = 7;
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
            var eventCutoff = DateTimeOffset.UtcNow;

            var players = (await _modelDb.Players
                .Where(p => p.Status == PlayerStatus.Normal).ToListAsync())
                .OrderBy(p => p != Player)
                .ThenBy(p => p.Details?.DisplayName ?? p.Details?.FirstName);

            var scoreboardEvents = (await _modelDb.Events
                .Where(e => e.TipsDeadline < eventCutoff).ToListAsync())
                .OrderBy(e => e.OrderKey);

            var results = (await (
                from r in _modelDb.Results
                join eId in scoreboardEvents.Select(e => e.Id) on r.Event.Id equals eId
                select r).ToListAsync()).ToLookup(r => r.Event.Id);

            var tips = (await (
                    from t in _modelDb.Tips
                    join eId in scoreboardEvents.Select(e => e.Id) on t.Target.Event.Id equals eId
                    group t by new { eId, t.Tipper.Id, t.Target.Type }
                ).ToListAsync())
                .Select(tipList => tipList.MaxBy(t => t.SubmittedAt)!)
                .ToLookup(t => (t.Tipper.Id, t.Target.Event.Id));

            foreach (var p in players)
            {
                Players.Add(new(p.Id, GetPlayerName(p)));
            }

            foreach (var e in scoreboardEvents)
            {
                var eventResults = results[e.Id].OrderBy(r => r.Type).ToList();
                Events.Add(new(e.Id, e.EventName, eventResults));

                foreach (var p in players)
                {
                    var playerEventTips = tips[(p.Id, e.Id)].ToList();
                    var report = _tipScoring.GetReport(e, playerEventTips);
                    if (report is not null && report.ScoredTips.Count > 0)
                    {
                        Reports[(p.Id, e.Id)] = report;
                    }
                }
            }

            return Page();
        }

        private string GetPlayerName(Player playerToDisplay)
        {
            string? displayName = null;
            if (playerToDisplay.Id == Player!.Id)
            {
                displayName = playerToDisplay.Details?.FirstName;
            }
            else
            {
                displayName = playerToDisplay.Details?.DisplayName;
                if (displayName is not null && displayName.Length > DISPLAY_NAME_LEN)
                {
                    displayName = $"{displayName[..DISPLAY_NAME_LEN]}(...)";
                }
                else
                {
                    displayName = playerToDisplay.Details?.FirstName;
                }
            }

            return displayName ?? "Unknown Player";
        }

        public record PlayerView(Guid Id, string Name);
        public record EventView(Guid Id, string Name, List<Result> Results);
    }
}
