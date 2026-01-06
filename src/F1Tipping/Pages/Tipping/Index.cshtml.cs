using Microsoft.AspNetCore.Mvc;
using F1Tipping.Model.Tipping;
using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Model;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.PlayerData;
using F1Tipping.Tipping;
using System.ComponentModel.DataAnnotations;
using F1Tipping.Platform;

namespace F1Tipping.Pages.Tipping;

[PlayerMustBeInitalized]
public class IndexModel(
    IConfiguration configuration,
    UserManager<User> userManager,
    ModelDbContext modelDb,
    TipScoringService scoreService,
    CurrentDataService currentData
    ) : PlayerPageModel(configuration, userManager, modelDb)
{
    [BindProperty]
    public IList<EventTipView> EventTips { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        var selectedSeasonId = AuthUser!.Settings.SystemSettings?.SelectedSeason ?? (await currentData.GetCurrentSeasonAsync()).Id;

        var events = Array.Empty<Event>()
            .Concat(await ModelDb.Seasons.Where(s => s.Id == selectedSeasonId).ToListAsync())
            .Concat(await ModelDb.Races.Where(r => r.Weekend.Season.Id == selectedSeasonId).ToListAsync())
            .OrderBy(e => e.OrderKey);
        var eventsToShowTipsFor = new List<Event>();
        var otherActivePlayers = new List<Player>();

        var nextRound = ((Race?)events.FirstOrDefault(
            e => e.TipsDeadline > DateTimeOffset.UtcNow && e is Race))?.Weekend;

        if (nextRound is not null)
        {
            eventsToShowTipsFor.AddRange(nextRound.Events.Where(e => e is IEventWithResults));
            if (nextRound.Index == 1 && nextRound.Season is IEventWithResults)
            {
                eventsToShowTipsFor.Add(nextRound.Season);
            }
            otherActivePlayers.AddRange(await ModelDb.Players
                .Where(p => p != Player && p.Status == PlayerStatus.Normal)
                .ToListAsync());
        }

        foreach (var e in events)
        {
            var scoreReport = await scoreService.GetReportAsync(Player!, e);
            var tipList = e is IEventWithResults
                ? await TipReportingService.GetTipsAsync(Player!, (e as IEventWithResults)!, ModelDb)
                : Array.Empty<Tip>();

            List<string>? tipHavers;
            List<string>? tipHaveNots;
            if (eventsToShowTipsFor.Contains(e))
            {
                tipHavers = new();
                tipHaveNots = new();
                foreach (var otherPlayer in otherActivePlayers)
                {
                    if ((await TipReportingService.GetTipsAsync(otherPlayer,
                        (e as IEventWithResults)!, ModelDb)).Any())
                    {
                        tipHavers.Add(otherPlayer.Details!.DisplayOrFirstName);
                    }
                    else
                    {
                        tipHaveNots.Add(otherPlayer.Details!.DisplayOrFirstName);
                    }
                }
                tipHavers.Sort();
                tipHaveNots.Sort();
            }
            else
            {
                tipHavers = null;
                tipHaveNots = null;
            }

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
                Score: scoreReport?.EventScore,
                PlayersWithTipsIn: tipHavers,
                PlayersWithNoTips: tipHaveNots
                ));
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

    public record EventTipView(
        Guid EventId,
        string Name,
        DateTimeOffset Deadline,
        [property: Display(Name = "You Tipped")]
        bool HasTips,
        decimal? Score,
        [property: Display(Name = "Have Tipped")]
        List<string>? PlayersWithTipsIn,
        [property: Display(Name = "Haven't Tipped")]
        List<string>? PlayersWithNoTips);
}
