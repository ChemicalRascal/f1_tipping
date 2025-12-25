using F1Tipping.Common;

namespace F1Tipping.Platform;

public partial class TippingDataSeedingService(
    IConfiguration configuration,
    IServiceProvider serviceProvider)
{
    private const string SEED_PATH_CONFIG_KEY = "SeedPath";

    public async Task ReadSeedFile()
    {
        var filepath = configuration.GetValue<string>(SEED_PATH_CONFIG_KEY);
        if (filepath is null)
        {
            return;
        }

        var data = DataFileReader.ReadFile(filepath);
        using (var scope = serviceProvider.CreateScope())
        {
            var persister = ActivatorUtilities.CreateInstance<DtoPersister>(scope.ServiceProvider);
            await persister.PersistDataSetAsync(data);
        }
    }
}

public partial class TippingDataSeedingService : IDefineCliArgs
{
    static IEnumerable<KeyValuePair<string, string>> IDefineCliArgs.Switches =>
        [ new("--seed-path", SEED_PATH_CONFIG_KEY) ];
}

public partial class TippingDataSeedingService : IHostedService
{
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        => await ReadSeedFile();

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
