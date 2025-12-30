using F1Tipping.Data;
using F1Tipping.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Shared.Components.SeasonSelector;

public class SeasonSelectorViewComponent(ModelDbContext modelDb) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var list = new SelectList(
            await modelDb.Seasons.ToListAsync(),
            nameof(Season.Id), nameof(Season.Year)
            );

        // TODO: This is giving a null ViewData object. Why?!!
        return View(list);
    }
}
