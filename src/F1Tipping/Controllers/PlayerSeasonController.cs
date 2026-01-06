using F1Tipping.Data;
using F1Tipping.Data.AppModel;
using F1Tipping.Platform;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace F1Tipping.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayerSeasonController(
    UserManager<User> userManager,
    ModelDbContext modelDb,
    AppDbContext appDb,
    CurrentDataService currentData
    ) : ControllerBase
{
    private async Task<User> GetDbUser()
        => await userManager.GetUserAsync(User) ?? throw new ApplicationException($"Null user for {User.Identity}");

    public record UpdateResponse(bool RequiresRefresh);

    // PUT api/<PlayerSeasonController>/<guid>
    [HttpPut("{seasonId}")]
    public async Task<UpdateResponse> Put(Guid seasonId)
    {
        var user = await GetDbUser();

        if (!(await modelDb.Seasons.AnyAsync(s => s.Id == seasonId)))
        {
            // TODO: Logging
            seasonId = (await currentData.GetCurrentSeasonAsync()).Id;
        }

        if (user.Settings.SystemSettings?.SelectedSeason == seasonId)
        {
            return new(false);
        }

        if (user.Settings.SystemSettings is null)
        {
            user.Settings.SystemSettings = new() { SelectedSeason = seasonId };
        }
        else
        {
            user.Settings.SystemSettings?.SelectedSeason = seasonId;
        }
        appDb.Update(user);
        await appDb.SaveChangesAsync();
        return new(true);
    }

    // GET api/<PlayerSeasonController>
    [HttpGet()]
    public async Task<Guid> Get()
    {
        var user = await GetDbUser();
        return user.Settings.SystemSettings?.SelectedSeason ?? (await currentData.GetCurrentSeasonAsync()).Id;
    }
}
