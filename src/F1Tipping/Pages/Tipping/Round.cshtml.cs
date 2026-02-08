using CsvHelper.Configuration.Attributes;
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
using Microsoft.IdentityModel.Tokens;

namespace F1Tipping.Pages.Tipping;

[PlayerMustBeInitalized]
public class RoundModel(
    IConfiguration configuration,
    UserManager<User> userManager,
    ModelDbContext modelDb,
    TipReportingService tipReportingService,
    TipValidiationService tipsValidation,
    TipScoringService tipsScoring
    ) : PlayerPageModel(configuration, userManager, modelDb)
{
    [BindProperty]
    [ValidateNever]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public required IList<EventView> Events { get; set; } = new List<EventView>();

    public class EventView()
    {
        [ValidateNever]
        public string EventName { get; set; } = string.Empty;
        public Guid EventId { get; set; } = default;
        public IList<TipView> Tips { get; set; } = [];
        [ValidateNever]
        public IDictionary<IEnumerable<Type>, SelectList> Candidates { get; set; } =
            new Dictionary<IEnumerable<Type>, SelectList>(new EnumerableComparer<Type>());
        [ValidateNever]
        public bool Lockout { get; set; } = false;
        [ValidateNever]
        public bool DisplayResults { get; set; } = false;
        public IDictionary<ResultType, TipScoringService.ScoredTip>? ScoreMap { get; set; } = null;
    }

    public record TipView(ResultType TargetType, Guid Selection) : IThinTip
    {
        ResultType IThinTip.Type => TargetType;
        Guid IThinTip.Selection => Selection;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var roundToTip = (await ModelDb.FindAsync<Round>(id))!;

        if (roundToTip.Events.IsNullOrEmpty())
        {
            return BadRequest();
        }

        foreach (var eventToTip in roundToTip.Events.OrderBy(e => e.OrderKey))
        {
            if (eventToTip is null || eventToTip is not IEventWithResults)
            {
                continue;
            }

            Events.Add(new()
            {
                EventName = eventToTip.EventName,
                EventId = eventToTip.Id,
                Tips = await BuildTipListAsync(eventToTip),
                Candidates = await GetCandidatesAsync(eventToTip),
                Lockout = eventToTip.TipsDeadline < DateTimeOffset.UtcNow,
                DisplayResults = eventToTip.Completed,
                ScoreMap = (await tipsScoring.GetReportAsync(Player!, eventToTip))?.ScoredTips,
            });
        }

        if (Events.IsNullOrEmpty())
        {
            return BadRequest();
        }

        return Page();
    }

    private async Task<IList<TipView>> BuildTipListAsync(Event eventToTip)
    {
        if (eventToTip is not IEventWithResults eWR)
        {
            return [];
        }

        var results = eWR.GetResultTypes().Select(
            rt => new Result() { Event = eventToTip, Type = rt }).ToList();
        var tips = await tipReportingService.GetTipsAsync(Player!, eWR);

        return (
            from result in results
            join tip in tips
                on (result.Event.Id, result.Type)
                equals (tip.Target.Event.Id, tip.Target.Type)
                into tipgroup
            from tip in tipgroup.DefaultIfEmpty()
            select new TipView(
                result.Type,
                tip?.Selection.Id ?? Guid.Empty)
            ).OrderBy(tip => tip.TargetType).ToList();
    }

    private async Task<Dictionary<IEnumerable<Type>, SelectList>>
        GetCandidatesAsync(Event eventToTip)
    {
        if (eventToTip is not IEventWithResults eWR)
        {
            return new(new EnumerableComparer<Type>());
        }
        return await GetCandidatesAsync(eWR.GetResultTypes());
    }

    private async Task<Dictionary<IEnumerable<Type>, SelectList>>
        GetCandidatesAsync(IEnumerable<ResultType> resultTypes)
    {
        var requiredCandidateSets = resultTypes.Select(ResultTypeHelper.RacingEntityTypes)
            .DistinctBy(a => a, new EnumerableComparer<Type>());

        var resolvedCandidates =
            new Dictionary<IEnumerable<Type>, SelectList>
            (new EnumerableComparer<Type>());

        var racingEntities = await ModelDb.RacingEntities.ToListAsync();

        foreach (var set in requiredCandidateSets)
        {
            resolvedCandidates[set] = new SelectList(
                set.SelectMany(reType =>
                    racingEntities
                    .Where(re => re.GetType() == reType && re.IsSelectable)
                    .OrderBy(re => re.GetListOrder())),
                nameof(RacingEntity.Id), nameof(RacingEntity.DisplayName));
        }

        return resolvedCandidates;
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var submissionTime = DateTimeOffset.UtcNow;

        var (validEvents, errorMessage) = await ValidateFatalErrors(Events, submissionTime);
        if (validEvents.IsNullOrEmpty())
        {
            StatusMessage = $"Error: {errorMessage}";
            return await ReloadPageAsync(id);
        }

        AddTippingErrorsToModelState(validEvents);
        if (!ModelState.IsValid)
        {
            StatusMessage = "Error: Tips are invalid.";
            return await ReloadPageAsync(id);
        }

        await PersistTipsAsync(validEvents);
        StatusMessage = "Tips saved.";
        return await ReloadPageAsync(id);

        async Task<IActionResult> ReloadPageAsync(Guid id)
        {
            var roundToTip = (await ModelDb.FindAsync<Round>(id))!;
            if (roundToTip is null)
            {
                return BadRequest();
            }
            Events.Clear();
            return await OnGetAsync(id);
        }
    }

    // TODO: Tip persistance service
    private async Task PersistTipsAsync(IList<EventView> tippedEvents)
    {
        foreach (var tippedEvent in tippedEvents)
        {
            var dbEvent = await ModelDb.FindAsync<Event>(tippedEvent.EventId);
            if (dbEvent is not IEventWithResults eventWithResults)
            {
                throw new ApplicationException();
            }

            var resultTypes = eventWithResults.GetResultTypes();
            var existingTips = await tipReportingService.GetTipsAsync(Player!, eventWithResults);
            var racingEntityIdMap = new Dictionary<Guid, RacingEntity>();

            foreach (var resultType in resultTypes)
            {
                var result = await ModelDb.FindAsync<Result>(tippedEvent.EventId, resultType);
                if (result is null)
                {
                    result = CreateNewResult(dbEvent, resultType);
                    await ModelDb.AddAsync(result);
                }

                var newTip = tippedEvent.Tips.SingleOrDefault(tip => tip.TargetType == resultType);
                if (newTip is null)
                {
                    continue;
                }

                var existingTip = existingTips.SingleOrDefault(tip => tip.Target.Type == resultType);
                if (newTip.Selection == existingTip?.Selection.Id)
                {
                    tippedEvent.Tips.Remove(newTip);
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
                tippedEvent.Tips.Remove(newTip);
            }

            if (tippedEvent.Tips.Any())
            {
                throw new ApplicationException(
                    $"{tippedEvent.Tips.Count} tips remaining after processing ({
                        string.Join(", ", tippedEvent.Tips.Select(t => t.TargetType))})");
            }

            TipReportingService.BustCache(Player!, dbEvent);
        }
        await ModelDb.SaveChangesAsync();
    }

    private void AddTippingErrorsToModelState(IList<EventView> validEvents)
    {
        foreach (var validEvent in validEvents)
        {
            var (i, _) = Events.Index().Single(p => p.Item.EventId == validEvent.EventId);
            var eventValidationErrors = tipsValidation.GetErrors(Events[i].Tips.Cast<IThinTip>());
            var indexableTipTypes = Events[i].Tips.Select(t => t.TargetType).ToList();

            foreach (var resultType in eventValidationErrors.Keys)
            {
                var j = indexableTipTypes.IndexOf(resultType);
                ModelState.AddModelError($"Events[{i}].Tips[{j}].Selection",
                    eventValidationErrors[resultType]);
            }
        }
    }

    private async Task<(IList<EventView> validEvents, string? ErrorMessage)>
        ValidateFatalErrors(IList<EventView> submittedEvents, DateTimeOffset submissionTime)
    {
        List<Guid> missingEventIds = [];
        List<Event> targetEvents = [];
        foreach (var postedEventId in submittedEvents.Select(e => e.EventId))
        {
            var e = await ModelDb.FindAsync<Event>(postedEventId);
            if (e is null)
            {
                missingEventIds.Add(postedEventId);
            }
            else
            {
                targetEvents.Add(e);
            }
        }
        if (missingEventIds.Count != 0)
        {
            return ([], $"Event ID(s) {string.Join(", ", missingEventIds)} don't exist!");
        }

        var illegalEvents = targetEvents.Where(e => e is not IEventWithResults);
        if (illegalEvents.Any())
        {
            return ([], $"Event type(s) {string.Join(", ", illegalEvents.Select(e => e.GetType()))} don't support tipping!");
        }

        var eventIdsWithinDeadline = targetEvents.Where(e => e.TipsDeadline >= submissionTime).Select(e => e.Id).ToList();
        if (eventIdsWithinDeadline.IsNullOrEmpty())
        {
            return ([], $"You're too late; the tipping deadline for all of these events have passed.");
        }

        return (submittedEvents.Where(e => eventIdsWithinDeadline.Contains(e.EventId)).ToList(), null);
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
}
