using CardsOfSpite.Api.Handlers;
using CardsOfSpite.Api.Hubs;
using CardsOfSpite.Api.Routes;
using CardsOfSpite.Api.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ClusterClientService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ClusterClientService>());
builder.Services.AddSingleton(sp => sp.GetRequiredService<ClusterClientService>().ClusterClient);
builder.Services.AddSingleton<MessageStreamListener>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MessageStreamListener>());
builder.Services.AddSignalR();
builder.Host.UseSerilog((ctx, log) =>
{
    log.Enrich.FromLogContext()
       .WriteTo.Console()
       .WriteTo.PostgreSQL(Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING"), "logs", needAutoCreateTable: true)
       .ReadFrom.Configuration(ctx.Configuration, "Serilog");
});

builder.Services.AddResponseCompression(o =>
{
    o.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
});

builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyOptions, ApiKeyHandler>("ApiKey", o => builder.Configuration.GetSection("ApiKeyOptions").Bind(o));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyOrigin()
    .AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDeckApi();
    endpoints.MapGameApi();
    endpoints.MapHub<GameHub>("/gamehub");
    endpoints.MapFallbackToFile("index.html");
});

Log.Information("Cards of Spite API starting");

app.Run();
