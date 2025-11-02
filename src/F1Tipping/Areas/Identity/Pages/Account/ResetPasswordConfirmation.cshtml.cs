using F1Tipping.Pages.PageModels;
using Microsoft.AspNetCore.Authorization;

namespace F1Tipping.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel(IConfiguration configuration)
        : BasePageModel(configuration)
    {
        public void OnGet()
        {
        }
    }
}
