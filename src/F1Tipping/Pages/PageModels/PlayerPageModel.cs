using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using F1Tipping.Model.Tipping;
using F1Tipping.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace F1Tipping.Pages.PageModels;

[Authorize(Roles = Role.Player)]
public abstract class PlayerPageModel(
    IConfiguration configuration,
    UserManager<User> userManager,
    ModelDbContext modelDb
    ) : BasePageModel(configuration)
{
    protected ModelDbContext ModelDb = modelDb;

    // TODO: Rework SetUserAsync call into being part of middleware, to
    // ensure that call is made after authentication in pipeline. Doing so
    // should allow us to just use normal properties here as well
    [BindProperty]
    public User? AuthUser
    { get { if (!_stateSet) { SetUserAsync(User).Wait(); }
            return _authUser; } }

    [BindProperty]
    public Player? Player
    { get { if (!_stateSet) { SetUserAsync(User).Wait(); }
            return _player; } }

    private bool _stateSet = false;
    private User? _authUser;
    private Player? _player;

    protected async Task SetUserAsync(ClaimsPrincipal userClaim)
    {
        _authUser = await userManager.GetUserAsync(userClaim);
        if (_authUser is not null)
        {
            _player = await ModelDb.Players.SingleOrDefaultAsync(
                p => p.AuthUserId == _authUser.Id);
        }
        _stateSet = true;
    }
}
