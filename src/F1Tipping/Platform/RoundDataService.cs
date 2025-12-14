using F1Tipping.Data;
using F1Tipping.Model;
using Microsoft.EntityFrameworkCore;

namespace F1Tipping.Platform;

public class RoundDataService(ModelDbContext gameDb)
{
    public async Task<Round?> GetSemanticNextRoundAsync()
    {
        var currentTime = DateTime.UtcNow;

        return await gameDb.Rounds.Where(r => r.StartDate > currentTime)
            .OrderBy(r => r.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Round?> GetRoundByIdAsync(Guid roundId)
        => await gameDb.Rounds.FirstOrDefaultAsync(r => r.Id == roundId);

    // TODO: Think up better terms than "Kind" and "Semantic"
    public async Task<Round?> GetKindPreviousRoundAsync() =>
        throw new NotImplementedException();

    public async Task<Round?> GetKindCurrentRoundAsync() => 
        throw new NotImplementedException();

    public async Task<Round?> GetKindNextRoundAsync() =>
        throw new NotImplementedException();
}
