using AspNetCoreGeneratedDocument;
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
    public abstract class PlayerPageModel : BasePageModel
    {
        private UserManager<IdentityUser<Guid>> _userManager;
        private AppDbContext _appDb;
        protected ModelDbContext _modelDb;

        // TODO: Rework SetUserAsync call into being part of middleware, to
        // ensure that call is made after authentication in pipeline. Doing so
        // should allow us to just use normal properties here as well
        [BindProperty]
        public IdentityUser<Guid>? AuthUser
        { get { if (!_stateSet) { SetUserAsync(User).Wait(); }
                return _authUser; } }
        [BindProperty]
        public Player? Player
        { get { if (!_stateSet) { SetUserAsync(User).Wait(); }
                return _player; } }

        private bool _stateSet = false;
        private IdentityUser<Guid>? _authUser;
        private Player? _player;

        protected PlayerPageModel(
            IConfiguration configuration,
            UserManager<IdentityUser<Guid>> userManager,
            AppDbContext appDb,
            ModelDbContext modelDb
            ) : base(configuration)
        {
            _userManager = userManager;
            _appDb = appDb;
            _modelDb = modelDb;
        }

        protected async Task SetUserAsync(ClaimsPrincipal userToSet)
        {
            _authUser = await _userManager.GetUserAsync(userToSet);
            if (_authUser is not null)
            {
                _player = await _modelDb.Players.SingleOrDefaultAsync(
                    p => p.AuthUserId == _authUser.Id);
            }
            _stateSet = true;
        }
    }
}
