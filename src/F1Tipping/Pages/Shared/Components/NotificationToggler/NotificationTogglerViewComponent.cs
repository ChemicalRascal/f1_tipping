using Microsoft.AspNetCore.Mvc;

namespace F1Tipping.Pages.Shared.Components.NotificationToggler;

public class NotificationTogglerViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View("Default");
    }
}