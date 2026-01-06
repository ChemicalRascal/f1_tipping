using F1Tipping.Data;
using F1Tipping.Model;
using F1Tipping.Data.AppModel;
using F1Tipping.Platform;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Pages.Shared.Components.SeasonSelector;

public class SeasonSelectorViewComponent(
    UserManager<User> userManager,
    ModelDbContext modelDb,
    CurrentDataService currentData
    ) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await userManager.GetUserAsync(UserClaimsPrincipal) ?? throw new ApplicationException($"Null user for {User.Identity}");
        var selectedSeasonId = user.Settings.SystemSettings?.SelectedSeason ?? (await currentData.GetCurrentSeasonAsync()).Id;

        var list = new SelectList(
            await modelDb.Seasons.ToListAsync(),
            nameof(Season.Id), nameof(Season.Year), selectedSeasonId
            );

        return View("Default", list);
    }
}
