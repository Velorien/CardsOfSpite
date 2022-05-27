using CardsOfSpite.GrainInterfaces;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace CardsOfSpite.Api.Services;

class ClusterClientService : IHostedService
{
    public IClusterClient ClusterClient { get; }

    public ClusterClientService()
    {
        ClusterClient = new ClientBuilder()
        .UseLocalhostClustering()
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev";
            options.ServiceId = "cardsofspite";
        })
        .AddSimpleMessageStreamProvider("SMS")
        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(GrainManifest).Assembly))
        .Build();
    }

    public Task StartAsync(CancellationToken cancellationToken) => ClusterClient.Connect();

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await ClusterClient.Close();
        ClusterClient.Dispose();
    }
}
