using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using F1Tipping.Pages.Admin.Players;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace F1Tipping.Pages.Admin.Users
{
    public class IndexModel : AdminPageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppDbContext _appDb;
        private readonly ModelDbContext _modelDb;
        private readonly UserManager<IdentityUser<Guid>> _userManager;

        public IndexModel(
            ILogger<IndexModel> logger,
            AppDbContext appDb,
            ModelDbContext modelDb,
            UserManager<IdentityUser<Guid>> userManager)
        {
            _logger = logger;
            _appDb = appDb;
            _modelDb = modelDb;
            _userManager = userManager;
        }

        [BindProperty]
        public IList<UserIndexEntry> Users { get; set; } = default!;
        [BindProperty]
        public string? StatusMessage { get; set; } = null;

        private async Task BuildUsersListAsync()
        {
            var users = await _appDb.Users.ToListAsync();
            var userRoles = await _appDb.UserRoles.ToListAsync();
            var roles = await _appDb.Roles.ToListAsync();
            var players = await _modelDb.Players.ToListAsync();

            Users = await Task.WhenAll(users.Select(async u =>
            {
                var p = players.SingleOrDefault(p => p.AuthUserId == u.Id);
                return new UserIndexEntry
                {
                    User = u,
                    Roles = roles.Join(
                        userRoles.Where(ur => ur.UserId == u.Id),
                        role => role.Id,
                        userRole => userRole.RoleId,
                        (role, userRole) => role).ToList(),
                    PlayerId = p?.Id,
                    PlayerName = p?.Details is not null
                        ? p?.Details?.FirstName
                            + (p?.Details.LastName is not null
                            ? " " + p!.Details.LastName
                            : string.Empty)
                        : null,
                    IsLocked = await _userManager.IsLockedOutAsync(u),
                };
            }));
        }

        public async Task OnGetAsync()
        {
            await BuildUsersListAsync();
        }

        public async Task<IActionResult> OnGetLockdownUserAsync(Guid? id)
        {
            if (!_userManager.SupportsUserLockout)
            {
                StatusMessage = "User lockdown is not supported.";
                await BuildUsersListAsync();
                return Page();
            }

            if (id is null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id.ToString() ?? string.Empty);
            if (user is null)
            {
                return NotFound();
            }

            var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (!lockoutResult.Succeeded)
            {
                StatusMessage = "User lockdown failed.";
                await BuildUsersListAsync();
                return Page();
            }

            await BuildUsersListAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetUnlockUserAsync(Guid? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id.ToString() ?? string.Empty);
            if (user is null)
            {
                return NotFound();
            }

            var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddDays(-1));
            if (!lockoutResult.Succeeded)
            {
                StatusMessage = "User unlock failed.";
            }

            await BuildUsersListAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetDeleteUserAsync(Guid? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id.ToString() ?? string.Empty);
            if (user is null)
            {
                return NotFound();
            }

            var player = await _modelDb.Players.SingleOrDefaultAsync(p => p.AuthUserId == id);
            if (player is not null && player.Status != Model.Tipping.PlayerStatus.Archived)
            {
                StatusMessage = "Player is not archived.";
                await BuildUsersListAsync();
                return Page();
            }

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                foreach (var e in deleteResult.Errors)
                {
                    _logger.LogError(e.Description);
                }
            }

            await BuildUsersListAsync();
            return Page();
        }

        public class UserIndexEntry
        {
            public required IdentityUser<Guid> User { get; set; }
            public IList<IdentityRole<Guid>> Roles { get; set; } = default!;
            public Guid? PlayerId { get; set; }
            [Display(Name="Player")]
            public string? PlayerName { get; set; }
            public bool IsLocked { get; set; }
        }
    }
}
