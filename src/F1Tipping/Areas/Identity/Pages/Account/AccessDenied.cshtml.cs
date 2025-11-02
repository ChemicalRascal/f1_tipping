using F1Tipping.Pages.PageModels;

namespace F1Tipping.Areas.Identity.Pages.Account;

public class AccessDeniedModel : BasePageModel
{
    public AccessDeniedModel(IConfiguration configuration) : base(configuration)
    { }

    public void OnGet()
    {
    }
}
