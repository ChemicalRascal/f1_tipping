using F1Tipping.Common;

namespace F1Tipping.Platform;

public partial class TippingDataSeedingService(
    IConfiguration configuration,
    IServiceProvider serviceProvider)
{
    private const string SEED_PATH_CONFIG_KEY = "SeedPath";

    public Task ReadSeedFile()
    {
        var filepath = configuration.GetValue<string>(SEED_PATH_CONFIG_KEY);
        if (filepath is null)
        {
            return Task.CompletedTask;
        }

        var fileReader = new DataFileReader();
        var data = fileReader.ReadFile(filepath);

        using (var scope = serviceProvider.CreateScope())
        {
            // Consume data object
        }

        return Task.CompletedTask;
    }
}

public partial class TippingDataSeedingService : IDefineCliArgs
{
    static IEnumerable<KeyValuePair<string, string>> IDefineCliArgs.Switches =>
        [ new("--seed-path", SEED_PATH_CONFIG_KEY) ];
}

public partial class TippingDataSeedingService : IHostedService
{
    Task IHostedService.StartAsync(CancellationToken cancellationToken)
        => ReadSeedFile();

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
