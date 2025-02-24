using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using F1Tipping.Model.Tipping;
using F1Tipping.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using F1Tipping.PlayerData;

namespace F1Tipping.Pages.PlayerAdmin
{
    [PlayerMustBeInitalized]
    public class IndexModel : PageModel
    {
        private readonly ModelDbContext _modelDb;
        private readonly UserManager<IdentityUser<Guid>> _userManager;

        public IndexModel(ModelDbContext modelDb, UserManager<IdentityUser<Guid>> userManager)
        {
            _modelDb = modelDb;
            _userManager = userManager;
        }

        [BindProperty]
        public Player? Player { get; set; }

        public async Task OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            Player = await _modelDb.Players.SingleOrDefaultAsync(p => p.AuthUserId == user!.Id);
        }
    }
}
