using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace F1Tipping.Pages.PageModels;

public class BasePageModel : PageModel
{
    [ViewData]
    public string? VapidPublicKey { get; set; }

    public BasePageModel(IConfiguration configuration)
    {
        VapidPublicKey = configuration.GetValue<string>("Vapid:publicKey");
    }
}
