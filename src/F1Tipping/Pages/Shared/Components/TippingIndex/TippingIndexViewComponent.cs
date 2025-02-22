using Microsoft.AspNetCore.Mvc;
using F1Tipping.Model.Tipping;

namespace F1Tipping.Pages.Shared.Components.TippingIndex
{
    public class TippingIndexViewComponent : ViewComponent
    {
        public TippingIndexViewComponent()
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(Player player)
        {
            return View();
        }
    }
}
