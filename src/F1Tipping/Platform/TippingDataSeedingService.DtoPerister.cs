using F1Tipping.Common;
using F1Tipping.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace F1Tipping.Platform;

public partial class TippingDataSeedingService
{
    private class DtoPersister(ModelDbContext modelDb)
    {
        public async Task PersistDataSetAsync(DataSet data)
        {
            await using var dataSetTransaction = await modelDb.Database.BeginTransactionAsync();

            try
            {
                await PersistSeasonsAsync(data.Seasons);
                await PersistRoundsAsync(data.Rounds);
                await PersistSprintRacesAsync(data.SprintRaces);
                await PersistTeamsAsync(data.Teams);
                await PersistDriversAsync(data.Drivers);
                await dataSetTransaction.CommitAsync();
            }
            catch
            {
                await dataSetTransaction.RollbackAsync();
                throw;
            }
            finally
            {
                await dataSetTransaction.DisposeAsync();
            }
        }

        private static DateTimeOffset Earliest(params DateTimeOffset[] dateTimes)
        {
            if (dateTimes.Length <= 0)
            {
                throw new ArgumentException($"{nameof(dateTimes)} must be provided");
            }
            return dateTimes.Aggregate(DateTimeOffset.MaxValue, (a, b) => (a < b ? a : b));
        }

        private async Task PersistSeasonsAsync(IEnumerable<Season> seasons)
        {
            var dbSeasons = await modelDb.Seasons.ToListAsync();

            foreach (var season in seasons)
            {
                var dbSeason = dbSeasons.Find(s => s.Year == new Year(season.Year));

                if (dbSeason is not null && !season.Persist)
                {
                    modelDb.Remove(dbSeason);
                    continue;
                }

                if (dbSeason is null && season.Persist)
                {
                    await modelDb.Seasons.AddAsync(new Model.Season()
                    {
                        Year = new(season.Year),
                        TipsDeadline = DateTimeOffset.MaxValue.UtcDateTime,
                    });
                }
            }

            await modelDb.SaveChangesAsync();
        }

        private async Task PersistRoundsAsync(IEnumerable<Round> rounds)
        {
            foreach (var round in rounds)
            {
                var dbSeason = await modelDb.Seasons
                    .SingleOrDefaultAsync(s => s.Year == new Year(round.Year))
                    ?? throw new ApplicationException($"Can't persist Round with Year {round.Year}");
                await modelDb.Entry(dbSeason).Collection(s => s.Rounds).LoadAsync();

                var dbRound = dbSeason.Rounds
                    .SingleOrDefault(r => r.Index == round.Index);

                if (round.Title is null || round.RoundStart is null)
                {
                    if (dbRound is not null)
                    {
                        dbSeason.Rounds.Remove(dbRound);
                        continue;
                    }
                }
                else
                {
                    if (dbRound is null)
                    {
                        dbRound = new()
                        {
                            Season = dbSeason,
                            Index = round.Index,
                            Title = round.Title,
                            StartDate = round.RoundStart.Value.UtcDateTime,
                            Events = [],
                        };
                        dbSeason.Rounds.Add(dbRound);
                        modelDb.Update(dbSeason);
                    }
                    else
                    {
                        dbRound.Title = round.Title;
                        dbRound.StartDate = round.RoundStart.Value.UtcDateTime;
                    }
                }

                var dbMainRace = dbRound!.Events
                    .SingleOrDefault(r => r.Type == Model.RaceType.Main);
                var dbSprint = dbRound!.Events
                    .SingleOrDefault(r => r.Type == Model.RaceType.Sprint);
                var sprintRaceTipDeadline = dbSprint?.TipsDeadline ?? DateTimeOffset.MaxValue.UtcDateTime;

                if (round.QualiStart is null || round.RaceStart is null)
                {
                    if (dbMainRace is not null)
                    {
                        dbRound.Events.Remove(dbMainRace);
                        continue;
                    }
                }
                else
                {
                    if (dbMainRace is null)
                    {
                        dbMainRace = new()
                        {
                            Weekend = dbRound,
                            Type = Model.RaceType.Main,
                            QualificationStart = round.QualiStart.Value.UtcDateTime,
                            RaceStart = round.RaceStart.Value.UtcDateTime,
                            TipsDeadline = Earliest(round.QualiStart.Value, sprintRaceTipDeadline).UtcDateTime,
                        };
                        dbRound.Events.Add(dbMainRace);

                        if (dbRound.Id != default)
                        {
                            modelDb.Update(dbRound);
                        }
                    }
                    {
                        dbMainRace.QualificationStart = round.QualiStart.Value.UtcDateTime;
                        dbMainRace.RaceStart = round.RaceStart.Value.UtcDateTime;
                        dbMainRace.TipsDeadline = Earliest(round.QualiStart.Value, sprintRaceTipDeadline).UtcDateTime;
                    }

                    if (dbSeason.TipsDeadline > dbMainRace.TipsDeadline)
                    {
                        dbSeason.TipsDeadline = Earliest(dbSeason.TipsDeadline, dbMainRace.TipsDeadline);
                        modelDb.Update(dbSeason);
                    }
                    if (dbSprint is not null)
                    {
                        dbSprint.TipsDeadline = Earliest(dbMainRace.TipsDeadline, dbSprint.TipsDeadline);
                        modelDb.Update(dbSprint);
                    }
                }
            }

            await modelDb.SaveChangesAsync();
        }

        private async Task PersistSprintRacesAsync(IEnumerable<SprintRace> sprints)
        {
            foreach (var sprint in sprints)
            {
                var dbSeason = await modelDb.Seasons
                    .SingleOrDefaultAsync(s => s.Year == new Year(sprint.Year))
                    ?? throw new ApplicationException($"Can't persist Sprint with Year {sprint.Year}");
                await modelDb.Entry(dbSeason).Collection(s => s.Rounds).LoadAsync();

                var dbRound = dbSeason.Rounds
                    .SingleOrDefault(r => r.Index == sprint.RoundIndex)
                    ?? throw new ApplicationException($"Can't persist Sprint with Round Index {sprint.RoundIndex}");

                var dbSprint = dbRound.Events.SingleOrDefault(r => r.Type == Model.RaceType.Sprint);
                var dbMainRace = dbRound!.Events.SingleOrDefault(r => r.Type == Model.RaceType.Main);
                var mainRaceTipDeadline = dbMainRace?.TipsDeadline ?? DateTimeOffset.MaxValue.UtcDateTime;

                if (sprint.QualiStart is null || sprint.RaceStart is null)
                {
                    if (dbSprint is not null)
                    {
                        dbRound.Events.Remove(dbSprint);
                        continue;
                    }
                }
                else
                {
                    if (dbSprint is null)
                    {
                        dbSprint = new()
                        {
                            Weekend = dbRound,
                            Type = Model.RaceType.Sprint,
                            QualificationStart = sprint.QualiStart.Value.UtcDateTime,
                            RaceStart = sprint.RaceStart.Value.UtcDateTime,
                            TipsDeadline = Earliest(sprint.QualiStart.Value, mainRaceTipDeadline).UtcDateTime,
                        };
                        dbRound.Events.Add(dbSprint);
                        modelDb.Update(dbRound);
                    }
                    else if (dbSprint is not null)
                    {
                        dbSprint.QualificationStart = sprint.QualiStart!.Value.UtcDateTime;
                        dbSprint.RaceStart = sprint.RaceStart!.Value.UtcDateTime;
                        dbSprint.TipsDeadline = Earliest(sprint.QualiStart.Value, mainRaceTipDeadline).UtcDateTime;
                        modelDb.Update(dbSprint);
                    }

                    if (dbSeason.TipsDeadline > dbSprint!.TipsDeadline)
                    {
                        dbSeason.TipsDeadline = Earliest(dbSeason.TipsDeadline, dbSprint.TipsDeadline);
                        modelDb.Update(dbSeason);
                    }
                    if (dbMainRace is not null)
                    {
                        dbMainRace.TipsDeadline = Earliest(dbMainRace.TipsDeadline, dbSprint.TipsDeadline);
                        modelDb.Update(dbMainRace);
                    }
                }
            }

            await modelDb.SaveChangesAsync();
        }

        private async Task PersistTeamsAsync(IEnumerable<Team> teams)
        {
            foreach (var team in teams)
            {
                var dbTeam = await modelDb.Teams
                    .IgnoreAutoIncludes()
                    .Include(t => t.DriverTeams)
                    .Where(t => t.Name == team.Name)
                    .SingleOrDefaultAsync();

                if (dbTeam is not null)
                {
                    if (!team.Active)
                    {
                        if (dbTeam.DriverTeams?.Count == 0)
                        {
                            modelDb.Remove(dbTeam);
                        }
                        else
                        {
                            dbTeam.Status = Model.EntityStatus.Inactive;
                            foreach (var driverTeam in dbTeam.DriverTeams ?? [])
                            {
                                driverTeam.Status = Model.EntityStatus.Inactive;
                            }
                            modelDb.Update(dbTeam);
                        }
                        continue;

                    }
                    else
                    {
                        dbTeam.Status = Model.EntityStatus.Active;
                        modelDb.Update(dbTeam);
                    }
                }
                else if (team.Active)
                {
                    dbTeam = new()
                    {
                        Name = team.Name,
                        Status = Model.EntityStatus.Active,
                    };
                    modelDb.Add(dbTeam);
                }
            }

            await modelDb.SaveChangesAsync();
        }

        private async Task PersistDriversAsync(IEnumerable<Driver> drivers)
        {
            foreach (var driver in drivers)
            {
                var dbDriver = await modelDb.Drivers
                    .IgnoreAutoIncludes()
                    .Include(d => d.DriverTeams)
                    .ThenInclude(dt => dt.Team)
                    .Where(d => d.FirstName == driver.FirstName && d.LastName == driver.LastName)
                    .SingleOrDefaultAsync();

                if (dbDriver is not null)
                {
                    if (!driver.Active)
                    {
                        dbDriver.Status = Model.EntityStatus.Inactive;
                        foreach (var driverTeam in dbDriver.DriverTeams)
                        {
                            driverTeam.Status = Model.EntityStatus.Inactive;
                        }
                        continue;
                    }
                }
                else
                {
                    dbDriver = new()
                    {
                        FirstName = driver.FirstName,
                        LastName = driver.LastName,
                        Nationality = driver.Nationality,
                        Number = driver.Number,
                        Status = driver.Active ? Model.EntityStatus.Active
                                               : Model.EntityStatus.Inactive,
                    };
                    modelDb.Add(dbDriver);
                }

                var referencedDriverTeam = dbDriver.DriverTeams
                    .Where(dt => dt.Team.Name == driver.Team)
                    .SingleOrDefault();

                foreach (var dt in dbDriver.DriverTeams)
                {
                    if (dt != referencedDriverTeam)
                    {
                        dt.Status = Model.EntityStatus.Inactive;
                    }
                    modelDb.Update(dt);
                }

                if (referencedDriverTeam is not null)
                {
                    referencedDriverTeam.Status = driver.Active
                        ? Model.EntityStatus.Active : Model.EntityStatus.Inactive;
                    modelDb.Update(referencedDriverTeam);
                }
                else
                {
                    var dbTeam = await modelDb.Teams
                        .IgnoreAutoIncludes()
                        .Where(t => t.Name == driver.Team)
                        .SingleOrDefaultAsync()
                        ?? throw new ApplicationException($"Can't find Team {driver.Team} for \"{driver.FirstName} {driver.LastName}\"");
                    referencedDriverTeam = new()
                    {
                        Driver = dbDriver,
                        Team = dbTeam,
                        Status = driver.Active
                            ? Model.EntityStatus.Active : Model.EntityStatus.Inactive,
                    };
                    dbDriver.DriverTeams.Add(referencedDriverTeam);
                    modelDb.Add(referencedDriverTeam);
                }
            }

            await modelDb.SaveChangesAsync();
        }
    }
}
