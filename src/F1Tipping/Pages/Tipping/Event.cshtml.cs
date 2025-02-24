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
        public required IList<ResultAndTipView> ResultsAndTips { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var @event = (await _modelDb.FindAsync<Event>(id))!;
            if (@event is null || @event is not IEventWithResults)
            {
                return BadRequest();
            }

            EventId = id;

            var results = ((IEventWithResults)@event).GetResultTypes().Select(
                rt => new Result() { Event = @event, Type = rt });
            var tips = _tipsService.GetTips(Player!, (IEventWithResults)@event);

            ResultsAndTips = (
                from result in results
                join tip in tips on result equals tip.Target into tipgroup
                from tip in tipgroup.DefaultIfEmpty()
                select new ResultAndTipView(
                    EventId,
                    result.Type,
                    tip?.Debug_Tip ?? string.Empty
                    )
                ).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            throw new NotImplementedException();
        }

        public record ResultAndTipView(Guid EventId, ResultType Target, string Tip);

        public class EventEditModel : Event
        {
            public EventEditModel() { Id = Guid.Empty; }
            public override float OrderKey => throw new NotImplementedException();
        }
    }
}
