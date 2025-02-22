using Microsoft.AspNetCore.Mvc;
using F1Tipping.Model.Tipping;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Model;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;

namespace F1Tipping.Pages.Tipping
{
    public class IndexModel : PlayerPageModel
    {
        public IndexModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb)
            : base(userManager, appDb, modelDb)
        { }

        [BindProperty]
        public IList<Round> Rounds { get; set; } = default!;

        public async Task<IActionResult> OnGet()
        {
            Rounds = await _modelDb.Rounds.ToListAsync();

            return Page();
        }
    }
}
