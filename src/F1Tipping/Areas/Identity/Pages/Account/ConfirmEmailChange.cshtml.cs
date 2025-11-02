using System.Text;
using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace F1Tipping.Areas.Identity.Pages.Account;

public class ConfirmEmailChangeModel : BasePageModel
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly SignInManager<IdentityUser<Guid>> _signInManager;

    public ConfirmEmailChangeModel(
        IConfiguration configuration,
        UserManager<IdentityUser<Guid>> userManager,
        SignInManager<IdentityUser<Guid>> signInManager
        ) : base(configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
    {
        if (userId == null || email == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ChangeEmailAsync(user, email, code);
        if (!result.Succeeded)
        {
            StatusMessage = "Error changing email.";
            return Page();
        }

        // Users log in with their email address, but usernames and email
        // addresses are distinct in the model.
        var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
        if (!setUserNameResult.Succeeded)
        {
            StatusMessage = "Error changing user name.";
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Thank you for confirming your email change.";
        return Page();
    }
}
