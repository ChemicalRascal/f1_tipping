using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Model.Tipping;
using Microsoft.AspNetCore.Authorization;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;

namespace F1Tipping.Pages.Admin.Users
{
    public class IndexModel : AdminPageModel
    {
        private readonly AppDbContext _appContext;
        private readonly ModelDbContext _modelContext;

        public IndexModel(AppDbContext appContext, ModelDbContext modelContext)
        {
            _appContext = appContext;
            _modelContext = modelContext;
        }

        public IList<UserIndexEntry> Users { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var users = await _appContext.Users.ToListAsync();
            var userRoles = await _appContext.UserRoles.ToListAsync();
            var roles = await _appContext.Roles.ToListAsync();
            var players = await _modelContext.Players.ToListAsync();

            Users = users.Select(u =>
                new UserIndexEntry
                {
                    User = u,
                    Roles = roles.Join(
                        userRoles.Where(ur => ur.UserId == u.Id),
                        role => role.Id,
                        userRole => userRole.RoleId,
                        (role, userRole) => role).ToList(),
                    PlayerId = players.SingleOrDefault(p => p.AuthUserId == u.Id)?.Id,
                }).ToList();
        }

        public class UserIndexEntry
        {
            public required IdentityUser<Guid> User { get; set; }
            public IList<IdentityRole<Guid>> Roles { get; set; } = default!;
            public Guid? PlayerId { get; set; }
        }
    }
}
