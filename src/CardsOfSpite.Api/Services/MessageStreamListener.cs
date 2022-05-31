using CardsOfSpite.Api.Hubs;
using CardsOfSpite.HubClient;
using CardsOfSpite.Models.Messages;
using Microsoft.AspNetCore.SignalR;
using Orleans;
using Orleans.Streams;

namespace CardsOfSpite.Api.Services;

public class MessageStreamListener : IHostedService, IAsyncObserver<Message>
{
    private readonly IClusterClient _cluster;
    private readonly IHubContext<GameHub, IGameHubClient> _hub;
    private readonly Dictionary<Guid, StreamSubscriptionHandle<Message>> _handles = new();
    private IStreamProvider? _streamProvider;

    public MessageStreamListener(IClusterClient cluster, IHubContext<GameHub, IGameHubClient> hub)
    {
        _cluster = cluster;
        _hub = hub;
    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex) => Task.CompletedTask;

    public async Task OnNextAsync(Message item, StreamSequenceToken token = null!)
    {
        var target = item is TargetedMessage tm
            ? _hub.Clients.Client(tm.PlayerId)
            : _hub.Clients.Group(item.GameId.ToString());

        await (item switch
        {
            WaitingForPlayersMessage m => target.WaitingForPlayers(m),
            PlayerLeftQueueMessage m => target.PlayerLeftQueue(m),
            PlayerLeftGameMessage m => target.PlayerLeftGame(m),
            WinnerSelectedMessage m => target.WinnerSelected(m),
            HandDiscardedMessage m => target.HandDiscarded(m),
            CardsSelectedMessage m => target.CardsSelected(m),
            RoundStartedMessage m => target.RoundStarted(m),
            PlayerJoinedMessage m => target.PlayerJoined(m),
            RevealCardsMessage m => target.RevealCards(m),
            GameEndedMessage m => target.GameEnded(m),
            HandSetMessage m => target.HandSet(m),
            _ => Task.CompletedTask
        });
    }

    public async Task RegisterGame(Guid gameId)
    {
        if (_handles.ContainsKey(gameId)) return;

        var observable = _streamProvider!.GetStream<Message>(gameId, null);
        _handles[gameId] = await observable.SubscribeAsync(this);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _streamProvider = _cluster.GetStreamProvider("SMS");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
