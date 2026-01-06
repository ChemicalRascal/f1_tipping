using F1Tipping.Common;
using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using F1Tipping.Pages.PageModels;
using F1Tipping.PlayerData;
using F1Tipping.Tipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace F1Tipping.Pages.Tipping;

[PlayerMustBeInitalized]
public class EventModel(
    IConfiguration configuration,
    UserManager<User> userManager,
    ModelDbContext modelDb,
    TipValidiationService tipsValidation,
    TipScoringService tipsScoring
    ) : PlayerPageModel(configuration, userManager, modelDb)
{
    // TODO: Put these into ViewData or something, wtf
    [ValidateNever]
    [BindProperty]
    public string? EventTitle { get; set; } = default!;
    [BindProperty]
    public Guid EventId { get; set; } = Guid.Empty;
    [BindProperty]
    public required IList<TipView> IncomingTips { get; set; } = default!;
    [BindProperty]
    [ValidateNever]
    public IList<IList<SelectListItem>> TipSelections { get; set; } = default!;
    [BindProperty]
    [ValidateNever]
    public string? StatusMessage { get; set; }
    [BindProperty]
    [ValidateNever]
    public bool Lockout { get; set; } = false;
    [BindProperty]
    [ValidateNever]
    public bool DisplayResults { get; set; } = false;
    [BindProperty]
    [ValidateNever]
    public IDictionary<ResultType, TipScoringService.ScoredTip>? ReportMap { get; set; } = null;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var eventToTip = (await ModelDb.FindAsync<Event>(id))!;
        if (eventToTip is null || eventToTip is not IEventWithResults)
        {
            return BadRequest();
        }

        await RefreshTipStateAsync(eventToTip);

        return Page();
    }

    private async Task RefreshTipStateAsync(Event eventToTip)
    {
        EventId = eventToTip.Id;
        EventTitle = BuildEventName(eventToTip);

        var results = ((IEventWithResults)eventToTip).GetResultTypes().Select(
            rt => new Result() { Event = eventToTip, Type = rt }).ToList();
        var tips = await TipReportingService.GetTipsAsync(Player!, (IEventWithResults)eventToTip, ModelDb);

        IncomingTips = (
            from result in results
            join tip in tips
                on (result.Event.Id, result.Type)
                equals (tip.Target.Event.Id, tip.Target.Type)
                into tipgroup
            from tip in tipgroup.DefaultIfEmpty()
            select new TipView(
                result.Type,
                tip?.Selection.Id ?? Guid.Empty
                )
            ).OrderBy(tip => tip.TargetType).ToList();

        var resolvedCandidates = await GetCandidates(results.Select(r => r.Type));

        TipSelections = results
            .Select(r => ResultTypeHelper.RacingEntityTypes(r.Type))
            .Select(s => resolvedCandidates[s]).ToList();

        Lockout = eventToTip.TipsDeadline < DateTimeOffset.UtcNow;
        DisplayResults = eventToTip.Completed;

        if (DisplayResults)
        {
            ReportMap = (await tipsScoring.GetReportAsync(Player!, eventToTip))?.ScoredTips;
        }
    }

    private async Task<Dictionary<IEnumerable<Type>, IList<SelectListItem>>>
        GetCandidates(IEnumerable<ResultType> resultTypes)
    {
        var requiredCandidateSets = resultTypes.Select(ResultTypeHelper.RacingEntityTypes)
            .DistinctBy(a => a, new EnumerableComparer<Type>());

        var resolvedCandidates =
            new Dictionary<IEnumerable<Type>, IList<SelectListItem>>
            (new EnumerableComparer<Type>());

        foreach (var set in requiredCandidateSets)
        {
            resolvedCandidates[set] = new List<SelectListItem>();
            foreach (var reType in set)
            {
                resolvedCandidates[set].AddRange(
                    (await ModelDb.RacingEntities.ToListAsync())
                    .Where(re => re.GetType() == reType && re.IsSelectable)
                    .OrderBy(re => re.GetListOrder())
                    .Select(re => new SelectListItem(re.DisplayName, re.Id.ToString())));
            }
        }

        return resolvedCandidates;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // TODO: Chase down this bizarre fucking error.
        ModelState.Remove("");

        var targetEvent = await ModelDb.FindAsync<Event>(EventId);
        if (targetEvent is null)
        {
            StatusMessage = $"Event ID {EventId} doesn't exist!";
            return Page();
        }
        if (targetEvent is not IEventWithResults eventWithResults)
        {
            StatusMessage = $"Event type {targetEvent!.GetType()} doesn't support tipping!";
            return Page();
        }

        if (targetEvent.TipsDeadline < DateTimeOffset.UtcNow)
        {
            StatusMessage = $"You're too late; the tipping deadline for this event has passed.";
            await RefreshTipStateAsync(targetEvent);
            return Page();
        }

        var validationErrors = tipsValidation.GetErrors(
            IncomingTips.Select(t => (IThinTip)t).ToList());

        foreach (var eKey in validationErrors.Keys)
        {
            ModelState.AddModelError($"IncomingTips[{
                IncomingTips.IndexOf(IncomingTips.First(t => t.TargetType == eKey))
                }].Selection", validationErrors[eKey]);
        }

        if (!ModelState.IsValid)
        {
            StatusMessage = "Submission failed.";
            var eventToTip = (await ModelDb.FindAsync<Event>(EventId))!;
            if (eventToTip is null || eventToTip is not IEventWithResults)
            {
                return BadRequest();
            }
            await RefreshTipStateAsync(eventToTip);
            return Page();
        }

        var resultTypes = eventWithResults.GetResultTypes();
        var existingTips = await TipReportingService.GetTipsAsync(Player!, eventWithResults, ModelDb);
        var racingEntityIdMap = new Dictionary<Guid, RacingEntity>();

        foreach (var resultType in resultTypes)
        {
            var result = await ModelDb.FindAsync<Result>(EventId, resultType);
            if (result is null)
            {
                result = CreateNewResult(targetEvent, resultType);
                await ModelDb.AddAsync(result);
            }

            var newTip = IncomingTips.SingleOrDefault(tip => tip.TargetType == resultType);
            if (newTip is null)
            {
                continue;
            }

            var existingTip = existingTips.SingleOrDefault(tip => tip.Target.Type == resultType);
            if (newTip.Selection == existingTip?.Selection.Id)
            {
                IncomingTips.Remove(newTip);
                continue;
            }

            if (!racingEntityIdMap.TryGetValue(newTip.Selection, out var selection))
            {
                selection = await ModelDb.FindAsync<RacingEntity>(newTip.Selection);
                if (selection is null)
                {
                    throw new ApplicationException($"Can't find RacingEntity {newTip.Selection}");
                }
                racingEntityIdMap[newTip.Selection] = selection;
            }

            var allowedTypes = ResultTypeHelper.RacingEntityTypes(newTip.TargetType);
            if (!allowedTypes.Contains(selection.GetType()))
            {
                throw new ApplicationException($"RacingEntity {newTip.Selection} not valid for {newTip.TargetType}");
            }

            await ModelDb.AddAsync(new Tip()
            {
                SubmittedAt = DateTimeOffset.UtcNow,
                SubmittedBy_AuthUser = AuthUser!.Id,
                Tipper = Player!,
                Target = result,
                Selection = selection,
            });
            IncomingTips.Remove(newTip);
        }

        if (IncomingTips.Any())
        {
            throw new ApplicationException(
                $"{IncomingTips.Count} tips remaining after processing ({
                    IncomingTips.First().TargetType}{
                    (IncomingTips.Count > 1 ? ", ..." : string.Empty )})");
        }

        TipReportingService.BustCache(Player!, targetEvent);
        await ModelDb.SaveChangesAsync();

        await RefreshTipStateAsync(targetEvent);

        StatusMessage = "Tips saved.";

        return Page();
    }

    private Result CreateNewResult(Event targetEvent, ResultType resultType)
    {
        var resultObj = Activator.CreateInstance(ResultTypeHelper.ResultStructure(resultType));
        if (resultObj is null)
        {
            throw new ApplicationException($"{nameof(CreateNewResult)} couldn't create a valid result object for type {resultType}!");
        }

        var result = (Result)resultObj;
        result.Event = targetEvent;
        result.Type = resultType;
        return result;
    }

    private static string BuildEventName(Event e)
    {
        return e switch
        {
            Season s => $"{s.Year} Season",
            Race r => $"{r.Weekend.Season.Year}, Round {r.Weekend.Index} - {r.Type switch
            {
                RaceType.Main => "Main Race",
                RaceType.Sprint => "Sprint Race",
                _ => throw new NotImplementedException(),
            }} - {r.Weekend.Title}",
            _ => throw new NotImplementedException(),
        };
    }

    public record TipView(ResultType TargetType, Guid Selection) : IThinTip
    {
        ResultType IThinTip.Type => TargetType;
        Guid IThinTip.Selection => Selection;
    }
}
