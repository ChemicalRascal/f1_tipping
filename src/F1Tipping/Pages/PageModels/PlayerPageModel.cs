using F1Tipping.Data;
using F1Tipping.Model.Tipping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace F1Tipping.Pages.PageModels
{
    [Authorize(Roles = "Player")]
    public abstract class PlayerPageModel : PageModel
    {
        private UserManager<IdentityUser<Guid>> _userManager;
        private AppDbContext _appDb;
        protected ModelDbContext _modelDb;

        [BindProperty]
        public IdentityUser<Guid>? AuthUser { get; set; }
        [BindProperty]
        public Player? Player { get; set; }

        protected PlayerPageModel(
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb
            )
        {
            _userManager = userManager;
            _appDb = appDb;
            _modelDb = modelDb;
        }

        protected async Task SetUserAsync(ClaimsPrincipal userToSet)
        {
            AuthUser = await _userManager.GetUserAsync(userToSet);
            if (AuthUser is not null)
            {
                Player = await _modelDb.Players.SingleOrDefaultAsync(p => p.AuthUserId == AuthUser.Id);
            }
        }
    }
}
