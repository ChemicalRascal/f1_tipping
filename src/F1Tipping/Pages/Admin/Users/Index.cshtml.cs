using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace F1Tipping.Pages.Admin.Users
{
    public class IndexModel : AdminPageModel
    {
        private readonly AppDbContext _appContext;
        private readonly ModelDbContext _modelContext;
        private readonly UserManager<IdentityUser<Guid>> _userManager;

        public IndexModel(
            AppDbContext appContext,
            ModelDbContext modelContext,
            UserManager<IdentityUser<Guid>> userManager)
        {
            _appContext = appContext;
            _modelContext = modelContext;
            _userManager = userManager;
        }

        [BindProperty]
        public IList<UserIndexEntry> Users { get; set; } = default!;
        [BindProperty]
        public string? StatusMessage { get; set; } = null;

        private async Task BuildUsersListAsync()
        {
            var users = await _appContext.Users.ToListAsync();
            var userRoles = await _appContext.UserRoles.ToListAsync();
            var roles = await _appContext.Roles.ToListAsync();
            var players = await _modelContext.Players.ToListAsync();

            Users = await Task.WhenAll(users.Select(async u =>
                new UserIndexEntry
                {
                    User = u,
                    Roles = roles.Join(
                        userRoles.Where(ur => ur.UserId == u.Id),
                        role => role.Id,
                        userRole => userRole.RoleId,
                        (role, userRole) => role).ToList(),
                    PlayerId = players.SingleOrDefault(p => p.AuthUserId == u.Id)?.Id,
                    IsLocked = await _userManager.IsLockedOutAsync(u),
            }));
        }

        public async Task OnGetAsync()
        {
            await BuildUsersListAsync();
        }

        public async Task<IActionResult> OnGetLockdownUser(Guid? id)
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

        public async Task<IActionResult> OnGetUnlockUser(Guid? id)
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
                await BuildUsersListAsync();
                return Page();
            }

            await BuildUsersListAsync();
            return Page();
        }

        public class UserIndexEntry
        {
            public required IdentityUser<Guid> User { get; set; }
            public IList<IdentityRole<Guid>> Roles { get; set; } = default!;
            public Guid? PlayerId { get; set; }
            public bool IsLocked { get; set; }
        }
    }
}
