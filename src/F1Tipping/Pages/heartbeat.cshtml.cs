using F1Tipping.Pages.PageModels;

namespace F1Tipping.Pages;

public class HeartbeatModel(IConfiguration configuration)
    : BasePageModel(configuration)
{
    public void OnGet()
    {
    }
}