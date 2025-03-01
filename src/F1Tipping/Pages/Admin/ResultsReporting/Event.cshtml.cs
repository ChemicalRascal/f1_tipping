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
using System.Reflection;

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
        [ValidateNever]
        [BindProperty]
        public string EventTitle { get; set; } = default!;
        [BindProperty]
        public Guid EventId { get; set; }
        [BindProperty]
        public List<ResultViewModel> Results { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                StatusMessage = $"{ModelState.ErrorCount} errors in ModelState.";
                return Page();
            }

            var @event = await _modelDb.FindAsync<Event>(EventId);
            if (@event is null)
            {
                StatusMessage = $"Event not found.";
                return Page();
            }

            _modelDb.RemoveRange(await _modelDb.Results.Where(r => r.Event == @event).ToListAsync());
            await _modelDb.SaveChangesAsync();

            var resultsAdded = 0;
            foreach (var rg in Results.GroupBy(r => r.TypeOfResult))
            {
                if (rg.Any(r => r.HolderId != Guid.Empty))
                {
                    var resultObj = Activator.CreateInstance(ResultTypeHelper.ResultStructure(rg.Key));
                    if (resultObj is null)
                    {
                        throw new ApplicationException($"Couldn't get result object for {rg.Key}");
                    }
                    var result = (Result)resultObj;

                    result.Event = @event;
                    result.Type = rg.Key;
                    result.Set = DateTimeOffset.UtcNow;
                    result.SetByAuthUser = (await _userManager.GetUserAsync(User))!.Id;

                    var holderIds = rg.Select(rv => rv.HolderId);
                    var holders = await _modelDb.RacingEntities.Where(re => holderIds.Contains(re.Id)).ToListAsync();
                    if (resultObj is MultiEntityResult multiResult)
                    {
                        if (multiResult.ResultHolders is null)
                        {
                            multiResult.ResultHolders = new();
                        }
                        multiResult.ResultHolders.AddRange(holders);
                        _modelDb.Add(multiResult);
                        resultsAdded++;
                    }
                    else
                    {
                        if (holders.Count() != 1)
                        {
                            throw new ApplicationException($"{holders.Count()} RacingEntities found, expected 1 for ResultType {rg.Key}.");
                        }
                        result.ResultHolder = holders.First();
                        _modelDb.Add(result);
                        resultsAdded++;
                    }
                }
            }

            await _modelDb.SaveChangesAsync();

            StatusMessage = $"Saved {resultsAdded} results.";
            return await OnGetAsync(EventId);
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

            EventTitle = BuildEventName(eventInDb);
            var requiredResultTypes = @event.GetResultTypes();
            var existingResults = await _modelDb.Results.Where(r => r.Event.Id == id).ToListAsync();
            var fullResults = existingResults.Concat(requiredResultTypes
                    .Where(rt => !existingResults.Any(r => r.Type == rt))
                    .Select(rt =>
                    {
                        var resultObj = Activator.CreateInstance(ResultTypeHelper.ResultStructure(rt));
                        if (resultObj is null)
                        {
                            throw new ApplicationException($"Can't build Result for {rt}");
                        }
                        var newResult = (Result)resultObj;
                        newResult.Event = eventInDb;
                        newResult.Type = rt;
                        return newResult;
                    })).OrderBy(r => r.Type);

            var candidates = await GetCandidates(requiredResultTypes);
            foreach (var types in candidates.Keys)
            {
                // TODO: Get null ID values working?
                candidates[types] = candidates[types].Prepend(new("Select One...", Guid.Empty.ToString())).ToList();
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
                                                .Select(c => new SelectListItem(c.Text, c.Value, c.Value == rh.Id.ToString())).ToList(),
                                    MultiEntity: true
                                    );
                            })
                            : [])
                            .Concat([new ResultViewModel(
                                TypeOfResult: multiResult.Type,
                                HolderId: null,
                                Candidates: candidates[ResultTypeHelper.RacingEntityTypes(multiResult.Type)],
                                MultiEntity: true
                            )]),
                    Result singleResult => [new ResultViewModel(
                            TypeOfResult: singleResult.Type,
                            HolderId: singleResult.ResultHolder?.Id,
                            Candidates: candidates[ResultTypeHelper.RacingEntityTypes(singleResult.Type)]
                                        .Select(c => new SelectListItem(c.Text, c.Value, c.Value == singleResult?.ResultHolder?.Id.ToString())).ToList(),
                            MultiEntity: false
                            )]
                }).SelectMany(x => x).ToList();

            return Page();
        }

        private async Task<Dictionary<IEnumerable<Type>, IList<SelectListItem>>>
            GetCandidates(IEnumerable<ResultType> resultTypes)
        {
            var requiredCandidateSets = resultTypes
                .Select(ResultTypeHelper.RacingEntityTypes)
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
                        .OrderBy(re => re.GetListOrder())
                        .Select(re => new SelectListItem(re.DisplayName, re.Id.ToString())));
                }
            }

            return resolvedCandidates;
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

        public record ResultViewModel(
            [property: Display(Name = "Result")]
            ResultType TypeOfResult,
            [property: Display(Name = "Holder")]
            Guid? HolderId,
            [ValidateNever]
            [property: Display(Name = "Candidates")]
            IList<SelectListItem> Candidates,
            [ValidateNever]
            [property: Display(Name = "Multi-Entity Result")]
            bool MultiEntity
            );
    }
}
