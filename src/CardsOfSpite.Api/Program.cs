using CardsOfSpite.Api.Handlers;
using CardsOfSpite.Api.Hubs;
using CardsOfSpite.Api.Routes;
using CardsOfSpite.Api.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ClusterClientService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<ClusterClientService>());
builder.Services.AddSingleton(sp => sp.GetRequiredService<ClusterClientService>().ClusterClient);
builder.Services.AddSingleton<MessageStreamListener>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MessageStreamListener>());
builder.Services.AddSignalR();
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

app.UseAuthorization();
app.UseAuthentication();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDeckApi();
    endpoints.MapGameApi();
    endpoints.MapHub<GameHub>("/gamehub");
    endpoints.MapFallbackToFile("index.html");
});

app.Run();
