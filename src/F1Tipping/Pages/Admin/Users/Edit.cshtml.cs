using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using F1Tipping.Data;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;

namespace F1Tipping.Pages.Admin.Users
{
    public class EditModel : AdminPageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser<Guid>> _userManager;

        public EditModel(AppDbContext context, UserManager<IdentityUser<Guid>> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public UserView UserToEdit { get; set; } = default!;
        [BindProperty]
        public string? StatusMessage { get; set; } = null;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            UserToEdit = new UserView(user.Id, user.Email);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByIdAsync(UserToEdit.Id.ToString());
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{UserToEdit.Id}'.");
            }

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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(UserToEdit.Id))
                {
                    return BadRequest();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
