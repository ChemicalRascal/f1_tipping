using Microsoft.AspNetCore.Mvc;
using F1Tipping.Model.Tipping;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Model;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.PlayerData;
using F1Tipping.Tipping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace F1Tipping.Pages.Admin.ResultsReporting
{
    public class IndexModel : AdminPageModel
    {
        private TipReportingService _tips;
        private ModelDbContext _modelDb;
        private AppDbContext _appDb;
        private UserManager<IdentityUser<Guid>> _userManager;

        public IndexModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb,
            TipReportingService tips)
        {
            _tips = tips;
            _modelDb = modelDb;
            _appDb = appDb;
            _userManager = userManager;
        }

        [BindProperty]
        public IEnumerable<EventView> Events { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            var events = (await _modelDb.Events.ToListAsync()).OrderBy(e => e.OrderKey);

            Events = events.Select(e => new EventView(
                    EventId: e.Id,
                    Completed: e.Completed,
                    Name: e switch
                    {
                        Season s => $"{s.Year} Season",
                        Race r => BuildRaceName(r),
                        _ => throw new NotImplementedException(),
                    },
                    TimeUntilQuali: e switch
                    {
                        Season s => null,
                        Race r => r.QualificationStart - DateTimeOffset.UtcNow,
                        _ => throw new NotImplementedException(),
                    },
                    TimeUntilRace: e switch
                    {
                        Season s => null,
                        Race r => r.RaceStart - DateTimeOffset.UtcNow,
                        _ => throw new NotImplementedException(),
                    }));

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

        public record EventView(
            Guid EventId,
            [property: Display(Name = "Completed")]
            bool Completed,
            [property: Display(Name = "Event")]
            string Name,
            [property: Display(Name = "Time Until Quali")]
            TimeSpan? TimeUntilQuali,
            [property: Display(Name = "Time Until Race")]
            TimeSpan? TimeUntilRace);
    }
}
