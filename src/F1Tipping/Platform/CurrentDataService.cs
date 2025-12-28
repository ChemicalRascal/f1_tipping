using F1Tipping.Data;
using F1Tipping.Model;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Platform;

public class CurrentDataService(ModelDbContext modelDb)
{
    private Round? currentRound;

    public async Task<Season> GetCurrentSeasonAsync()
        => await modelDb.Seasons.SingleOrDefaultAsync(s => s.Year.Equals(DateTimeOffset.UtcNow))
        ?? await modelDb.Seasons.Where(s => s.Year < DateTimeOffset.UtcNow).OrderBy(s => s.Year).LastAsync();

    public async Task<Round?> GetRoundByIdAsync(Guid roundId)
        => await modelDb.Rounds.FirstOrDefaultAsync(r => r.Id == roundId);

    public async Task<Round?> GetCurrentRoundAsync()
    {
        if (currentRound is not null)
        {
            return currentRound;
        }

        var currentTime = DateTimeOffset.UtcNow;
        currentRound = await modelDb.Rounds.SingleOrDefaultAsync(
            r => r.StartDate < currentTime
            && r.EndDate > currentTime);
        return currentRound;
    }

    public async Task<Round?> GetNextRoundAsync()
    {
        var currentTime = DateTimeOffset.UtcNow;

        return await modelDb.Rounds.Where(r => r.StartDate > currentTime)
            .OrderBy(r => r.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Round?> GetPreviousRoundAsync()
    {
        var currentTime = DateTimeOffset.UtcNow;

        return await modelDb.Rounds.Where(r => r.EndDate < currentTime)
            .OrderBy(r => r.StartDate)
            .LastOrDefaultAsync();
    }
}
