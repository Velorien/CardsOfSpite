// See https://aka.ms/new-console-template for more information
using CardsOfSpite.Grains;
using Orleans.Configuration;
using Orleans;
using Orleans.Hosting;
using Serilog;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = Host
    .CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {
        silo.Configure<ClusterOptions>(o =>
            {
                o.ClusterId = "dev";
                o.ServiceId = "cardsofspite";
            })
            .ConfigureApplicationParts(
                parts => parts
                    .AddApplicationPart(typeof(GrainManifest).Assembly)
                    .WithReferences())
            .UseLocalhostClustering()
            .AddSimpleMessageStreamProvider("SMS")
            .AddMemoryGrainStorage("PubSubStore")
            .AddAzureBlobGrainStorageAsDefault(o =>
            {
                o.UseJson = true;
                o.ConfigureBlobServiceClient("UseDevelopmentStorage=true");
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
