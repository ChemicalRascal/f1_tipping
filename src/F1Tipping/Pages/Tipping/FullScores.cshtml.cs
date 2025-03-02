using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using F1Tipping.Data;

namespace F1Tipping.Pages.Tipping
{
    public class FullScoresModel : PlayerPageModel
    {
        public FullScoresModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb)
            : base(userManager, appDb, modelDb)
        {
        }

        public async Task<IActionResult> OnGet()
        {
            throw new NotImplementedException();
        }
    }
}
