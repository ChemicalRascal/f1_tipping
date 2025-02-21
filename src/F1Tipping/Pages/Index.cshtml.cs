using F1Tipping.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private ModelDbContext _modelDb;
        private UserManager<IdentityUser<Guid>> _userManager;

        public IndexModel(ILogger<IndexModel> logger, ModelDbContext modelDb, UserManager<IdentityUser<Guid>> userManager)
        {
            _logger = logger;
            _modelDb = modelDb;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet()
        {
            if ((User?.Identity?.IsAuthenticated ?? false) && User.IsInRole("Player"))
            {
                var user = await _userManager.GetUserAsync(User);
                var player = await _modelDb.Players.SingleOrDefaultAsync(p => p.AuthUserId == user!.Id);

                if (player is null || player.Status == Model.Tipping.PlayerStatus.Uninitialized)
                {
                    return Redirect("PlayerAdmin/Init");
                }
            }

            return Page();
        }
    }
}
