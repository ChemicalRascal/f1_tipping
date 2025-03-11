using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Admin.Data
{
    public class IndexModel : AdminPageModel
    {
        private DataSeeder _dataSeeder;
        private ModelDbContext _modelDb;

        [BindProperty]
        public IList<Event> Events { get; set; } = default!;
        [BindProperty]
        public IDictionary<Guid, string[]> PlayersWithTips { get; set; } = default!;

        public IndexModel(DataSeeder dataSeeder, ModelDbContext modelDb)
        {
            _dataSeeder = dataSeeder;
            _modelDb = modelDb;
        }

        public async Task OnGetAsync()
        {
            Events = (await _modelDb.Events.ToListAsync()).OrderBy(e => e.OrderKey).ToList();
            var tips = await _modelDb.Tips.ToListAsync();
            var players = await _modelDb.Players.ToListAsync();

            PlayersWithTips =
                (from e in Events
                 join t in tips on e equals t.Target.Event
                 group t.Tipper.Details?.FirstName ?? "??" by e.Id)
                 .ToDictionary(g => g.Key, g => g.Distinct().Order().ToArray());
        }

        public async Task OnGetSeedDataAsync()
        {
            await _dataSeeder.Seed2025SeasonAsync();
            await OnGetAsync();
        }
    }
}
