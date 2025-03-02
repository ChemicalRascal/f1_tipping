using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Tipping;

namespace F1Tipping.Pages.Tipping
{
    public class ScoreboardModel : PlayerPageModel
    {
        private TipScoringService _tipScoring;

        public ScoreboardModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb,
            TipScoringService tipScoring)
            : base(userManager, appDb, modelDb)
        {
            _tipScoring = tipScoring;
        }

        [BindProperty]
        public List<PlayerScoreView> PlayerScores { get; set; } = new();

        public async Task<IActionResult> OnGet()
        {
            var players = await _modelDb.Players.ToListAsync();

            foreach (var player in players)
            {
                if (player.Status != Model.Tipping.PlayerStatus.Normal)
                {
                    continue;
                }

                var score = await _tipScoring.GetPlayerScoreAsync(player);
                PlayerScores.Add(new(
                    player!.Details!.DisplayName ?? player.Details!.FirstName,
                    score));
            }

            PlayerScores = PlayerScores.OrderByDescending(ps => ps.Score).ToList();
            return Page();
        }

        public record PlayerScoreView(string Name, decimal Score);
    }
}
