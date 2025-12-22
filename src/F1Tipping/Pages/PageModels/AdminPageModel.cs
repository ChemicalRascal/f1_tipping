using F1Tipping.Data.AppModel;
using Microsoft.AspNetCore.Authorization;

namespace F1Tipping.Pages.PageModels
{
    [Authorize(Roles = Role.Administrator)]
    public abstract class AdminPageModel : BasePageModel
    {
        protected AdminPageModel(IConfiguration configuration) : base(configuration)
        {
        }
    }
}
