using Microsoft.AspNetCore.Mvc;
using F1Tipping.Data.AppModel;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using F1Tipping.Tipping;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Platform;
using F1Tipping.Data.ModelMigrations;

namespace F1Tipping.Pages.Tipping;

public class FullScoresModel(
    IConfiguration configuration,
    UserManager<User> userManager,
    ModelDbContext modelDb,
    TipScoringService tipScoring,
    CurrentDataService currentData
    ) : PlayerPageModel(configuration, userManager, modelDb)
{
    private const int DISPLAY_NAME_LEN = 7;

    [BindProperty]
    public Dictionary<(Guid Player, Guid Event), TipScoringService.PlayerEventReport> Reports { get; set; } = new();
    [BindProperty]
    public List<PlayerView> Players { get; set; } = new();
    [BindProperty]
    public List<EventView> Events { get; set; } = new();

    public async Task<IActionResult> OnGet(string? year = null)
    {
        var selectedSeasonId = AuthUser!.Settings.SystemSettings?.SelectedSeason ?? (await currentData.GetCurrentSeasonAsync()).Id;
        var eventCutoff = DateTimeOffset.UtcNow;

        var players = (await ModelDb.Players
            .Where(p => p.Status == PlayerStatus.Normal).ToListAsync())
            .OrderBy(p => p != Player)
            .ThenBy(p => p.Details?.DisplayName ?? p.Details?.FirstName)
            .ToList();

        var scoreboardEvents = Array.Empty<Event>()
            .Concat(await ModelDb.Seasons.Where(s => s.TipsDeadline < eventCutoff && s.Id == selectedSeasonId).ToListAsync())
            .Concat(await ModelDb.Races.Where(r => r.TipsDeadline < eventCutoff && r.Weekend.Season.Id == selectedSeasonId).ToListAsync())
            .OrderBy(e => e.OrderKey);

        var results = await (
            from r in ModelDb.Results
            join eId in scoreboardEvents.Select(e => e.Id) on r.Event.Id equals eId
            select r
            ).ToAsyncEnumerable()
            .ToLookupAsync(r => r.Event.Id);

        var raceTips = (
            from t in ModelDb.Tips
            join r in ModelDb.Races on t.Target.Event.Id equals r.Id
            where r.Weekend.Season.Id == selectedSeasonId
            orderby t.Target.Event.Id
            group t by new { eId = t.Target.Event.Id, tId = t.Tipper.Id, t.Target.Type }
            into g
            select g.OrderByDescending(t => t.SubmittedAt).First()
            ).ToAsyncEnumerable();

        var seasonTips = (
            from t in ModelDb.Tips
            join s in ModelDb.Seasons on t.Target.Event.Id equals s.Id
            where s.Id == selectedSeasonId
            orderby t.Target.Event.Id
            group t by new { eId = t.Target.Event.Id, tId = t.Tipper.Id, t.Target.Type }
            into g
            select g.OrderByDescending(t => t.SubmittedAt).First()
            ).ToAsyncEnumerable();

        var tips = await raceTips.Concat(seasonTips).ToLookupAsync(t => (pId: t.Tipper.Id, eId: t.Target.Event.Id));

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
                var playerEventTips = tips[(pId: p.Id, eId: e.Id)].ToList();
                var report = tipScoring.GetReportFromTips(e, playerEventTips);
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
