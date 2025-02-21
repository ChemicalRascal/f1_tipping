using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace F1Tipping.Pages.Admin.Users
{
    public class EditModel : AdminPageModel
    {
        private readonly AppDbContext _appDb;
        private readonly UserManager<IdentityUser<Guid>> _userManager;

        public EditModel(AppDbContext appDb, UserManager<IdentityUser<Guid>> userManager)
        {
            _appDb = appDb;
            _userManager = userManager;
        }

        [BindProperty]
        public UserView UserToEdit { get; set; } = default!;
        [BindProperty]
        public string? StatusMessage { get; set; } = null;
        [BindProperty]
        public List<UserRoleView> UserRoles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appDb.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            UserToEdit = new UserView(user.Id, user.Email);

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _appDb.Roles.ToListAsync();

            allRoles.ForEach(r =>
                UserRoles.Add(new(
                    user.Id,
                    r.Name ?? string.Empty,
                    userRoles.Contains(r.Name ?? string.Empty))));

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                StatusMessage = "ModelState is not valid.";
                return Page();
            }

            var user = await _userManager.FindByIdAsync(UserToEdit.Id.ToString());
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{UserToEdit.Id}'.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (UserToEdit.Email != user.Email)
            {
                var result = await _userManager.SetEmailAsync(user, UserToEdit.Email);
                if (!result.Succeeded)
                {
                    StatusMessage = "Error changing email.";
                    return Page();
                }

                var setUserNameResult = await _userManager.SetUserNameAsync(user, UserToEdit.Email);
                if (!setUserNameResult.Succeeded)
                {
                    StatusMessage = "Error changing user name.";
                    return Page();
                }

                user = await _userManager.FindByIdAsync(user.Id.ToString());
                if (user is null)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError,
                        "Could not retrieve user.");
                }
            }

            var newRoles = UserRoles.Where(ur => ur.UserInRole).Select(ur => ur.Role);

            if (!currentRoles.ToHashSet().SetEquals(newRoles))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(newRoles));
                await _userManager.AddToRolesAsync(user, newRoles.Except(currentRoles));
            }

            return RedirectToPage("./Index");
        }
    }
}
