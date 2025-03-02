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
            var playerList = (await _modelDb.Players.ToListAsync()).OrderBy(p => p == Player).ThenBy(p => p.Details?.DisplayName ?? p.Details?.FirstName).ToList();
            var eventList = (await _modelDb.Events.ToListAsync()).Where(e => e.TipsDeadline < DateTimeOffset.UtcNow).OrderBy(e => e.OrderKey);

            foreach (var p in playerList)
            {
                Players.Add(new(p.Id, p.Details?.DisplayName ?? p.Details?.FirstName ?? "Unknown Player!"));
            }

            foreach (var e in eventList)
            {
                var results = await _modelDb.Results.Where(r => r.Event == e).OrderBy(r => r.Type).ToListAsync();
                Events.Add(new(e.Id, e.EventName, results));

                foreach (var p in playerList)
                {
                    var report = await _tipScoring.GetReportAsync(p, e);
                    if (report is not null)
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
