using CardsOfSpite.GrainInterfaces;
using CardsOfSpite.HubClient;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace CardsOfSpite.Api.Hubs;

public class GameHub : Hub<IGameHubClient>
{
    private readonly IClusterClient _cluster;

    public GameHub(IClusterClient cluster) => _cluster = cluster;

    public async Task<bool> JoinGame(Guid gameId, string name)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        bool success = await _cluster.GetGrain<IGame>(gameId).Join(Context.ConnectionId, name);
        if (!success)
        {
            await Clients.Caller.Error("Failed to join the game");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        else
        {
            Context.Items["gameId"] = gameId;
        }

        return success;
    }

    public async Task SelectCards(Guid gameId, List<Guid> cardsIds)
    {
        bool success = await _cluster.GetGrain<IGame>(gameId).SelectCards(Context.ConnectionId, cardsIds);
        if (!success) await Clients.Caller.Error("Failed to select cards");
    }

    public Task SelectWinner(Guid gameId, string playerId) =>
        _cluster.GetGrain<IGame>(gameId).SelectWinner(playerId);

    public Task DiscardHand(Guid gameId, string playerId) =>
        _cluster.GetGrain<IGame>(gameId).DiscardHand(playerId);

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        if (Context.Items.TryGetValue("gameId", out var gameId))
        {
            await _cluster.GetGrain<IGame>((Guid)gameId!).Leave(Context.ConnectionId);
        }
    }
}
