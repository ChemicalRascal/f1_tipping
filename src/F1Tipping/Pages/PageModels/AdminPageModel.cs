using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Tipping.Pages.PageModels
{
    [Authorize(Roles = "Administrator")]
    public abstract class AdminPageModel : PageModel
    {
    }
}
