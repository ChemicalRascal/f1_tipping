using Microsoft.AspNetCore.Mvc;
using F1Tipping.Data.AppModel;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Tipping;
using F1Tipping.Platform;
using F1Tipping.Model;

namespace F1Tipping.Pages.Tipping;

public class ScoreboardModel(
    IConfiguration configuration,
    UserManager<User> userManager,
    ModelDbContext modelDb,
    TipScoringService tipScoring,
    CurrentDataService currentData
        ) : PlayerPageModel(configuration, userManager, modelDb)
{
    [BindProperty]
    public List<PlayerScoreView> PlayerScores { get; set; } = new();

    [BindProperty]
    public EventView? CurrentMainRace { get; set; }

    [BindProperty]
    public EventView? CurrentSprintRace { get; set; }

    [BindProperty]
    public string? PreviousRoundTitle { get; set; }

    [BindProperty]
    public string? CurrentRoundTitle { get; set; }

    [BindProperty]
    public EventView? Season { get; set; }

    public async Task<IActionResult> OnGet()
    {
        // TODO: Find a good way to only define this once.
        var selectedSeasonId = AuthUser!.Settings.SystemSettings?.SelectedSeason ?? (await currentData.GetCurrentSeasonAsync()).Id;
        var dbSeason = await ModelDb.Seasons.FindAsync(selectedSeasonId) ?? throw new ApplicationException($"No Season for ID {selectedSeasonId}");
        Season = new(dbSeason.ShortName, [..dbSeason.GetResultTypes()]);

        // TODO: As above.
        var currentRound = await currentData.GetCurrentRoundAsync(dbSeason) ?? await currentData.GetPreviousRoundAsync(dbSeason);
        if (currentRound is not null)
        {
            var main = currentRound.Events.SingleOrDefault(
                e => e is Race r && r.Type == RaceType.Main);
            if (main is not null)
            {
                CurrentMainRace = new(main.ShortName, [..main.GetResultTypes()]);
            }

            var sprint = currentRound.Events.SingleOrDefault(
                e => e is Race r && r.Type == RaceType.Sprint);
            if (sprint is not null)
            {
                CurrentSprintRace = new(sprint.ShortName, [..sprint.GetResultTypes()]);
            }

            var previousRound = await ModelDb.Rounds.Where(r => r.Index < currentRound.Index)
                .OrderBy(r => r.Index)
                .LastOrDefaultAsync();

            PreviousRoundTitle = previousRound?.Title;
            CurrentRoundTitle = $"Round {currentRound.Index}";
        }

        var players = await ModelDb.Players.ToListAsync();
        foreach (var player in players)
        {
            if (player.Status != Model.Tipping.PlayerStatus.Normal)
            {
                continue;
            }

            var midSeasonReport = await tipScoring.GetPlayerScoreAsync(player, selectedSeasonId);

            PlayerScores.Add(new(
                player!.Details!.DisplayName ?? player.Details!.FirstName,
                midSeasonReport.SummedScore,
                midSeasonReport));
        }

        PlayerScores = PlayerScores.OrderByDescending(ps => ps.SummedScore).ToList();
        return Page();
    }

    public record PlayerScoreView(string Name, decimal SummedScore, TipScoringService.PlayerSeasonReport ScoreData);

    public record EventView(string Name, List<ResultType> Results);
}
