using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Model.Tipping;
using F1Tipping.Pages.PageModels;
using F1Tipping.PlayerData;
using F1Tipping.Tipping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace F1Tipping.Pages.Tipping
{
    [PlayerMustBeInitalized]
    public class EventModel : PlayerPageModel
    {
        private TipReportingService _tipsService;

        public EventModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb,
            TipReportingService tipsService)
            : base(userManager, appDb, modelDb)
        {
            _tipsService = tipsService;
        }

        [BindProperty]
        public Guid EventId { get; set; } = Guid.Empty;
        [BindProperty]
        public required IList<TipView> IncomingTips { get; set; } = default!;
        [BindProperty]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var eventToTip = (await _modelDb.FindAsync<Event>(id))!;
            if (eventToTip is null || eventToTip is not IEventWithResults)
            {
                return BadRequest();
            }

            RefreshTipState(eventToTip);

            return Page();
        }

        private void RefreshTipState(Event eventToTip)
        {
            EventId = eventToTip.Id;

            var results = ((IEventWithResults)eventToTip).GetResultTypes().Select(
                rt => new Result() { Event = eventToTip, Type = rt }).ToList();
            var tips = _tipsService.GetTips(
                Player!, (IEventWithResults)eventToTip).ToList();

            IncomingTips = (
                from result in results
                join tip in tips
                    on (result.Event, result.Type)
                    equals (tip.Target.Event, tip.Target.Type)
                    into tipgroup
                    from tip in tipgroup.DefaultIfEmpty()
                select new TipView(
                    EventId,
                    result.Type,
                    tip?.Debug_Tip ?? string.Empty
                    )
                ).ToList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //await SetUserAsync(User);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // TODO: Expand single form to cover entire round? This is vestigial
            if (IncomingTips.Any(t => t.EventId != EventId))
            {
                return BadRequest();
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
            var existingTips = _tipsService.GetTips(Player!, eventWithResults);

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
                if (newTip.Tip == existingTip?.Debug_Tip)
                {
                    IncomingTips.Remove(newTip);
                    continue;
                }

                await _modelDb.AddAsync(new Tip()
                {
                    SubmittedAt = DateTimeOffset.UtcNow,
                    SubmittedBy_AuthUser = AuthUser!.Id,
                    Tipper = Player!,
                    Target = result,
                    Debug_Tip = newTip!.Tip,
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

            RefreshTipState(targetEvent);

            return Page();
        }

        public record TipView(Guid EventId, ResultType TargetType, string Tip);
    }
}
