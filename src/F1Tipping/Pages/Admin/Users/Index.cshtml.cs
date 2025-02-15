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
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<UserWithRoles> Users { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var users = await _context.Users.ToListAsync();
            var userRoles = await _context.UserRoles.ToListAsync();

            // TODO fix whatever is fucking this up
            Users = (await Task.WhenAll(
                users.Select(async u =>
                    new UserWithRoles
                    {
                        User = u,
                        Roles = await _context.Roles.Join(
                            userRoles.Where(ur => ur.UserId == u.Id),
                            role => role.Id,
                            userRole => userRole.RoleId,
                            (role, userRole) => role)
                        .ToListAsync(),
                    }))).ToList();
        }

        public class UserWithRoles
        {
            public required IdentityUser<Guid> User { get; set; }
            public IList<IdentityRole<Guid>> Roles { get; set; } = default!;
        }
    }
}
