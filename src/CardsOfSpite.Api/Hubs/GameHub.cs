using CardsOfSpite.GrainInterfaces;
using CardsOfSpite.HubClient;
using CardsOfSpite.Models;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace CardsOfSpite.Api.Hubs;

public class GameHub : Hub<IGameHubClient>
{
    private readonly IClusterClient _cluster;

    public GameHub(IClusterClient cluster) => _cluster = cluster;

    public async Task<GameSettings?> JoinGame(Guid gameId, string name)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        var game = _cluster.GetGrain<IGame>(gameId);
        bool success = await game.Join(Context.ConnectionId, name);
        if (!success)
        {
            await Clients.Caller.Error("Failed to join the game");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId.ToString());
            return null;
        }
        else
        {
            Context.Items["gameId"] = gameId;
        }

        return await game.GetGameSettings();
    }

    public async Task SelectCards(List<Guid> cardsIds)
    {
        bool success = await _cluster.GetGrain<IGame>(GameId).SelectCards(Context.ConnectionId, cardsIds);
        if (!success) await Clients.Caller.Error("Failed to select cards");
    }

    public Task SelectWinner(string playerId) =>
        _cluster.GetGrain<IGame>(GameId).SelectWinner(playerId);

    public Task DiscardHand() => _cluster.GetGrain<IGame>(GameId).DiscardHand(Context.ConnectionId);

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        if (Context.Items.TryGetValue("gameId", out var gameId))
        {
            await _cluster.GetGrain<IGame>((Guid)gameId!).Leave(Context.ConnectionId);
        }
    }

    private Guid GameId => (Guid)Context.Items["gameId"]!;
}
