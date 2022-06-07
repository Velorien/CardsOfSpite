using CardsOfSpite.GrainInterfaces;
using Orleans;
using Orleans.Clustering.AzureStorage;
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

        var azuriteHost = Environment.GetEnvironmentVariable("AZURITE_HOST") ?? "127.0.0.1";
        var connectionString = @$"UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://{azuriteHost}";

        ClusterClient = new ClientBuilder()
        .UseAzureStorageClustering((AzureStorageGatewayOptions o) => {
            o.ConfigureTableServiceClient(connectionString);
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
        await ClusterClient.Connect(async e => {
            _logger.LogInformation("Attempting to connect to orleans cluter");
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
