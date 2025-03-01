using F1Tipping.Common;
using F1Tipping.Data;
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

namespace F1Tipping.Pages.Tipping
{
    [PlayerMustBeInitalized]
    public class EventModel : PlayerPageModel
    {
        private TipReportingService _tipsReporting;
        private TipValidiationService _tipsValidation;

        public EventModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb,
            TipReportingService tipsService,
            TipValidiationService tipsValidation
            )
            : base(userManager, appDb, modelDb)
        {
            _tipsReporting = tipsService;
            _tipsValidation = tipsValidation;
        }

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

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var eventToTip = (await _modelDb.FindAsync<Event>(id))!;
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

            var results = ((IEventWithResults)eventToTip).GetResultTypes().Select(
                rt => new Result() { Event = eventToTip, Type = rt }).ToList();
            var tips = _tipsReporting.GetTips(
                Player!, (IEventWithResults)eventToTip).ToList();

            IncomingTips = (
                from result in results
                join tip in tips
                    on (result.Event, result.Type)
                    equals (tip.Target.Event, tip.Target.Type)
                    into tipgroup
                from tip in tipgroup.DefaultIfEmpty()
                select new TipView(
                    result.Type,
                    tip?.Selection.Id ?? Guid.Empty
                    )
                ).ToList();

            var resolvedCandidates = await GetCandidates(results.Select(r => r.Type));

            TipSelections = results
                .Select(r => ResultTypeHelper.RacingEntityTypes(r.Type))
                .Select(s => resolvedCandidates[s]).ToList();
        }

        private async Task<Dictionary<IEnumerable<Type>, IList<SelectListItem>>>
            GetCandidates(IEnumerable<ResultType> resultTypes)
        {
            var requiredCandidateSets = resultTypes.Select(
                r => ResultTypeHelper.RacingEntityTypes(r))
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
                        (await _modelDb.RacingEntities.ToListAsync())
                        .Where(re => re.GetType() == reType)
                        .Select(re => new SelectListItem(re.DisplayName, re.Id.ToString())));
                }
            }

            return resolvedCandidates;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var validationErrors = _tipsValidation.GetErrors(
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
                var eventToTip = (await _modelDb.FindAsync<Event>(EventId))!;
                if (eventToTip is null || eventToTip is not IEventWithResults)
                {
                    return BadRequest();
                }
                await RefreshTipStateAsync(eventToTip);
                return Page();
            }

            var targetEvent = await _modelDb.FindAsync<Event>(EventId);
            // TODO: Check round deadlines before submitting tips
            //if (targetEvent)
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

            var resultTypes = eventWithResults.GetResultTypes();
            var existingTips = _tipsReporting.GetTips(Player!, eventWithResults);
            var racingEntityIdMap = new Dictionary<Guid, RacingEntity>();

            foreach (var resultType in resultTypes)
            {
                var result = await _modelDb.FindAsync<Result>(EventId, resultType);
                if (result is null)
                {
                    result = new Result() { Event = targetEvent, Type = resultType };
                    await _modelDb.AddAsync(result);
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
                    selection = await _modelDb.FindAsync<RacingEntity>(newTip.Selection);
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

                await _modelDb.AddAsync(new Tip()
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

            await _modelDb.SaveChangesAsync();

            await RefreshTipStateAsync(targetEvent);

            return Page();
        }

        public record TipView(ResultType TargetType, Guid Selection) : IThinTip
        {
            ResultType IThinTip.Type => TargetType;
            Guid IThinTip.Selection => Selection;
        }
    }
}
