// See https://aka.ms/new-console-template for more information
using CardsOfSpite.Grains;
using Orleans.Configuration;
using Orleans;
using Orleans.Hosting;
using Serilog;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Orleans.Clustering.AzureStorage;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = Host
    .CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        var azuriteHost = Environment.GetEnvironmentVariable("AZURITE_HOST") ?? "127.0.0.1";
        var connectionString = @$"UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://{azuriteHost}";

        silo.Configure<ClusterOptions>(o =>
            {
                o.ClusterId = "cardsofspite";
                o.ServiceId = "cardsofspite";
            })
            .ConfigureApplicationParts(
                parts => parts
                    .AddApplicationPart(typeof(GrainManifest).Assembly)
                    .WithReferences())
            .UseAzureStorageClustering((AzureStorageClusteringOptions o) => {
                o.ConfigureTableServiceClient(connectionString);
            })
            .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
            .AddSimpleMessageStreamProvider("SMS")
            .AddMemoryGrainStorage("PubSubStore")
            .AddAzureBlobGrainStorageAsDefault(o =>
            {
                o.UseJson = true;
                o.ConfigureBlobServiceClient(connectionString);
                o.ConfigureJsonSerializerSettings = serializer =>
                {
                    serializer.TypeNameHandling = TypeNameHandling.None;
                };
            });
    })
    .ConfigureLogging(log => log.AddSerilog(dispose: true))
    .UseConsoleLifetime()
    .Build();

Log.Logger.Information("CardsOfSpite Silo starting");

await host.RunAsync();
