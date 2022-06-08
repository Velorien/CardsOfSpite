using CardsOfSpite.GrainInterfaces;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace CardsOfSpite.Api.Services;

class ClusterClientService : IHostedService
{
    private readonly ILogger<ClusterClientService> _logger;

    public IClusterClient ClusterClient { get; }

    public ClusterClientService(ILogger<ClusterClientService> logger)
    {
        _logger = logger;

        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");

        ClusterClient = new ClientBuilder()
        .UseAdoNetClustering((AdoNetClusteringClientOptions options) =>
        {
            options.Invariant = "Npgsql";
            options.ConnectionString = connectionString;
        })
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "cardsofspite";
            options.ServiceId = "cardsofspite";
        })
        .AddSimpleMessageStreamProvider("SMS")
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(GrainManifest).Assembly))
        .Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await ClusterClient.Connect(async e =>
        {
            _logger.LogWarning(e, "Attempt to connect to orleans cluter failed");
            await Task.Delay(1000);
            return true;
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await ClusterClient.Close();
        ClusterClient.Dispose();
    }
}
