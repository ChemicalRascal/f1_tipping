using Microsoft.AspNetCore.Mvc;
using F1Tipping.Data.AppModel;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Tipping;
using F1Tipping.Platform;

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

    public async Task<IActionResult> OnGet()
    {
        // TODO: Find a good way to only define this once.
        var selectedSeasonId = AuthUser!.Settings.SystemSettings?.SelectedSeason ?? (await currentData.GetCurrentSeasonAsync()).Id;
        var players = await ModelDb.Players.ToListAsync();

        foreach (var player in players)
        {
            if (player.Status != Model.Tipping.PlayerStatus.Normal)
            {
                continue;
            }

            var (completed, provisional) = await tipScoring.GetPlayerScoreAsync(player, selectedSeasonId);
            PlayerScores.Add(new(
                player!.Details!.DisplayName ?? player.Details!.FirstName,
                completed, provisional));
        }

        PlayerScores = PlayerScores.OrderByDescending(ps => ps.CompleteScore).ToList();
        return Page();
    }

    public record PlayerScoreView(string Name, decimal CompleteScore, decimal ProvisionalScore);
}
