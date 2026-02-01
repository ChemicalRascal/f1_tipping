using F1Tipping.Data;
using F1Tipping.Model;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Platform;

public class CurrentDataService(ModelDbContext modelDb)
{
    public async Task<Season> GetCurrentSeasonAsync()
    {
        var currentTime = DateTimeOffset.UtcNow;
        return await modelDb.Seasons.SingleOrDefaultAsync(s => s.Year.Equals(currentTime))
        ?? (await modelDb.Seasons.ToListAsync()).Where(s => s.Year < currentTime).OrderBy(s => s.Year).Last();
    }

    public async Task<Round?> GetRoundByIdAsync(Guid roundId)
        => await modelDb.Rounds.FirstOrDefaultAsync(r => r.Id == roundId);

    public async Task<Round?> GetCurrentRoundAsync(Season? season = null)
    {
        season ??= await GetCurrentSeasonAsync();

        if (season.Year == 2025)
        {
            return modelDb.Rounds.First(r =>
            r.Id == new Guid("019b5c94-9fcd-7465-84a2-538917d46c97"));
        }

        var currentTime = DateTimeOffset.UtcNow;
        return await modelDb.Rounds.SingleOrDefaultAsync(
            r => r.Season == season
            && r.StartDate < currentTime
            && r.EndDate > currentTime);
    }

    public async Task<Round?> GetNextRoundAsync(Season? season = null)
    {
        var currentTime = DateTimeOffset.UtcNow;
        season ??= await GetCurrentSeasonAsync();

        return await modelDb.Rounds
            .Where(r => r.Season == season && r.StartDate > currentTime)
            .OrderBy(r => r.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Round?> GetPreviousRoundAsync(Season? season = null)
    {
        var currentTime = DateTimeOffset.UtcNow;
        season ??= await GetCurrentSeasonAsync();

        return await modelDb.Rounds
            .Where(r => r.Season == season && r.EndDate < currentTime)
            .OrderBy(r => r.StartDate)
            .LastOrDefaultAsync();
    }
}
