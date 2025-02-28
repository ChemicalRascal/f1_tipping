using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1Tipping.Pages.PageModels;
using F1Tipping.Model;
using F1Tipping.Common;
using F1Tipping.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Packaging;

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetAsync(Guid id)
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
                    .Select(rt =>
                    {
                        // I did not have "Commit Crimes Against Types" on my bingo card for this project
                        var newResult = Activator.CreateInstance(ResultTypeHelper.ResultStructure(rt));
                        newResult!.GetType().GetProperty(nameof(Result.Event))!.SetValue(newResult, eventInDb);
                        newResult!.GetType().GetProperty(nameof(Result.Type))!.SetValue(newResult, rt);
                        return (Result)newResult;
                    })).OrderBy(r => r.Type);

            var candidates = await GetCandidates(requiredResultTypes);
            foreach (var types in candidates.Keys)
            {
                // TODO: Get null ID values working.
                candidates[types] = candidates[types].Prepend(new("Select One...", "null")).ToList();
            }

            Results = fullResults.Select(r => r switch
                {
                    MultiEntityResult multiResult => (multiResult.ResultHolders?.Any() ?? false
                            ? multiResult.ResultHolders.Select(rh =>
                            {
                                return new ResultViewModel(
                                    TypeOfResult: multiResult.Type,
                                    HolderId: rh.Id,
                                    Candidates: candidates[ResultTypeHelper.RacingEntityTypes(multiResult.Type)]
                                                .Select(c => new SelectListItem(c.Text, c.Value, c.Value == rh.Id.ToString())).ToList()
                                    );
                            })
                            : [])
                            .Concat([new ResultViewModel(
                                TypeOfResult: multiResult.Type,
                                HolderId: null,
                                Candidates: candidates[ResultTypeHelper.RacingEntityTypes(multiResult.Type)]
                            )]),
                    Result singleResult => [new ResultViewModel(
                            TypeOfResult: singleResult.Type,
                            HolderId: singleResult.ResultHolder?.Id,
                            Candidates: candidates[ResultTypeHelper.RacingEntityTypes(singleResult.Type)]
                                        .Select(c => new SelectListItem(c.Text, c.Value, c.Value == singleResult?.ResultHolder?.Id.ToString())).ToList()
                            )]
                }).SelectMany(x => x).ToList();

            return Page();
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

        public record ResultViewModel(
            [property: Display(Name = "Result")]
            ResultType TypeOfResult,
            [property: Display(Name = "Holder")]
            Guid? HolderId,
            [ValidateNever]
            [property: Display(Name = "Candidates")]
            IList<SelectListItem> Candidates
            );
    }
}
