using Microsoft.AspNetCore.Authorization;

namespace F1Tipping.Pages.PageModels
{
    [Authorize(Roles = "Administrator")]
    public abstract class AdminPageModel : BasePageModel
    {
        protected AdminPageModel(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
