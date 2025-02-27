using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1Tipping.Pages.PageModels;
using F1Tipping.Model;
using F1Tipping.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Admin.ResultsReporting
{
    public class EventModel : AdminPageModel
    {
        private ModelDbContext _modelDb;
        private AppDbContext _appDb;
        private UserManager<IdentityUser<Guid>> _userManager;

        public EventModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb)
        {
            _modelDb = modelDb;
            _appDb = appDb;
            _userManager = userManager;
        }

        [BindProperty]
        public string? StatusMessage { get; set; } = default;
        [BindProperty]
        public Guid EventId { get; set; }
        [BindProperty]
        public List<ResultViewModel> Results { get; set; } = new();

        public async Task<IActionResult> OnGet(Guid id)
        {
            EventId = id;
            var eventInDb = await _modelDb.FindAsync<Event>(EventId);
            if (eventInDb is null)
            {
                StatusMessage = $"Event with ID {EventId} not found.";
                return Page();
            }
            if (eventInDb is not IEventWithResults @event)
            {
                StatusMessage = $"Event with ID {EventId} has no results.";
                return Page();
            }

            var requiredResultTypes = @event.GetResultTypes();
            var existingResults = await _modelDb.Results.Where(r => r.Event.Id == id).ToListAsync();
            var fullResults = existingResults.Concat(
                requiredResultTypes
                    .Where(rt => !existingResults.Any(r => r.Type == rt))
                    .Select(rt => new Result()
                    {
                        Event = eventInDb,
                        Type = rt
                    })).OrderBy(r => r.Type);

            Results = fullResults.Select(r => r switch
                {
                    MultiEntityResult multiResult => (multiResult.ResultHolders?.Any() ?? false
                            ? multiResult.ResultHolders.Select(rh => new ResultViewModel(
                                TypeOfResult: multiResult.Type,
                                HolderId: rh.Id,
                                DEBUG: "Multi Result Path"
                                ))
                            : [])
                            .Concat([new ResultViewModel(
                                TypeOfResult: multiResult.Type,
                                HolderId: null,
                                DEBUG: "Extra Multi Result Path"
                            )]),
                    Result singleResult => [new ResultViewModel(
                            TypeOfResult: singleResult.Type,
                            HolderId: singleResult.ResultHolder?.Id,
                            DEBUG: "Single Result Path"
                            )]
                }).SelectMany(x => x).ToList();

            return Page();
        }

        public record ResultViewModel(
            [property: Display(Name = "Result")]
            ResultType TypeOfResult,
            [property: Display(Name = "Holder")]
            Guid? HolderId,
            [property: Display(Name = "Debug")]
            string DEBUG
            );
    }
}
