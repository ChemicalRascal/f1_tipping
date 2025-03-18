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
        private TipScoringService _scores;

        public IndexModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb,
            TipScoringService scores)
            : base(userManager, appDb, modelDb)
        {
            _scores = scores;
        }

        [BindProperty]
        public IList<EventTipView> EventTips { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            Console.WriteLine("EVENT TABLE GET");
            var events = (await _modelDb.Events.ToListAsync()).OrderBy(e => e.OrderKey);

            EventTips = new List<EventTipView>();
            foreach (var e in events)
            {
                var scoreReport = await _scores.GetReportAsync(Player!, e);
                var tipList = e is IEventWithResults
                    ? await TipReportingService.GetTipsAsync(Player!, (e as IEventWithResults)!, _modelDb)
                    : Array.Empty<Tip>();
                EventTips.Add(new EventTipView(
                    EventId: e.Id,
                    Name: e switch
                    {
                        Season s => $"{s.Year} Season",
                        Race r => BuildRaceName(r),
                        _ => throw new NotImplementedException(),
                    },
                    Deadline: e.TipsDeadline,
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

        public record EventTipView(Guid EventId,
                                   string Name,
                                   DateTimeOffset Deadline,
                                   bool HasTips,
                                   decimal? Score);
    }
}
