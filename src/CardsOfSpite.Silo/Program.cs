// See https://aka.ms/new-console-template for more information
using CardsOfSpite.Grains;
using Orleans.Configuration;
using Orleans;
using Orleans.Hosting;
using Serilog;
using Microsoft.Extensions.Hosting;

var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(connectionString, "logs", needAutoCreateTable: true)
    .CreateLogger();

var host = Host
    .CreateDefaultBuilder(args)
    .UseOrleans(silo =>
    {

        silo.Configure<ClusterOptions>(o =>
            {
                o.ClusterId = "cardsofspite";
                o.ServiceId = "cardsofspite";
            })
            .ConfigureApplicationParts(
                parts => parts
                    .AddApplicationPart(typeof(GrainManifest).Assembly)
                    .WithReferences())
            .UseAdoNetClustering((AdoNetClusteringSiloOptions options) =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
            })
            .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
            .AddSimpleMessageStreamProvider("SMS")
            .AddMemoryGrainStorage("PubSubStore")
            .AddAdoNetGrainStorageAsDefault((AdoNetGrainStorageOptions options) =>
            {
                options.Invariant = "Npgsql";
                options.ConnectionString = connectionString;
                options.UseFullAssemblyNames = false;
                options.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
                options.UseJsonFormat = true;
                options.IndentJson = true;
                options.UseFullAssemblyNames = false;
            });
    })
    .ConfigureLogging(log => log.AddSerilog(dispose: true))
    .UseConsoleLifetime()
    .Build();

Log.Logger.Information("CardsOfSpite Silo starting");

await host.RunAsync();
