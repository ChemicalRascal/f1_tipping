using Microsoft.AspNetCore.Mvc;
using F1Tipping.Model.Tipping;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Model;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.PlayerData;
using F1Tipping.Tipping;

namespace F1Tipping.Pages.Tipping
{
    [PlayerMustBeInitalized]
    public class IndexModel : PlayerPageModel
    {
        private TipReportingService _tips;
        private TipScoringService _scores;

        public IndexModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb,
            TipReportingService tips,
            TipScoringService scores)
            : base(userManager, appDb, modelDb)
        {
            _tips = tips;
            _scores = scores;
        }

        [BindProperty]
        public IList<EventTipView> EventTips { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            var events = (await _modelDb.Events.ToListAsync()).OrderBy(e => e.OrderKey);

            EventTips = new List<EventTipView>();
            foreach (var e in events)
            {
                var scoreReport = await _scores.GetReportAsync(Player!, e);
                var tipList = e is IEventWithResults
                    ? _tips.GetTips(Player!, (e as IEventWithResults)!)
                    : Array.Empty<Tip>();
                EventTips.Add(new EventTipView(
                    EventId: e.Id,
                    Name: e switch
                    {
                        Season s => $"{s.Year} Season",
                        Race r => BuildRaceName(r),
                        _ => throw new NotImplementedException(),
                    },
                    HasTips: tipList.Any(),
                    Score: scoreReport?.EventScore));
            }

            return Page();
        }

        private static string BuildRaceName(Race r)
        {
            return $"{r.Weekend.Season.Year}, Round {r.Weekend.Index} - {
                r.Type switch {
                    RaceType.Main => "Main Race",
                    RaceType.Sprint => "Sprint Race",
                    _ => throw new NotImplementedException(),
                }} - {r.Weekend.Title}";
        }

        public record EventTipView(Guid EventId, string Name, bool HasTips, int? Score);
    }
}
