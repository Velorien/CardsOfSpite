using CardsOfSpite.HubClient;
using CardsOfSpite.Models;
using CardsOfSpite.Models.Messages;
using CardsOfSpite.Web.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace CardsOfSpite.Web.Services;

public class GameService : IGameHubClient, IAsyncDisposable
{
    private readonly HubConnection _connection;

    public event Action StateChanged = null!;

    public bool Joined { get; private set; }
    public bool Waiting { get; private set; } = true;
    public bool CardsRevealed { get; private set; }
    public string CzarId { get; private set; } = null!;
    public BlackCard? BlackCard { get; private set; }
    public string PlayerId { get; private set; } = null!;
    public string? ErrorMessage { get; private set; }
    public IEnumerable<WhiteCard> Hand { get; private set; } = Enumerable.Empty<WhiteCard>();
    public IEnumerable<PlayerInfo> Players { get; private set; } = Enumerable.Empty<PlayerInfo>();
    public IEnumerable<PlayerInfo> PlayerQueue { get; private set; } = Enumerable.Empty<PlayerInfo>();
    public WinningSet? WinningSet { get; private set; }
    public Dictionary<string, List<WhiteCard>> SelectedCards { get; } = new();

    public bool IsCzar => PlayerId == CzarId;

    public GameService(NavigationManager navigation)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5001/gamehub")
            .Build();

        var hubMethods = typeof(IGameHubClient).GetMethods();
        
        _connection.On<WaitingForPlayersMessage>(nameof(WaitingForPlayers), WaitingForPlayers);
        _connection.On<WinnerSelectedMessage>(nameof(WinnerSelected), WinnerSelected);
        _connection.On<CardsSelectedMessage>(nameof(CardsSelected), CardsSelected);
        _connection.On<PlayerJoinedMessage>(nameof(PlayerJoined), PlayerJoined);
        _connection.On<RoundStartedMessage>(nameof(RoundStarted), RoundStarted);
        _connection.On<PlayerLeftQueueMessage>(nameof(PlayerLeftQueue), PlayerLeftQueue);
        _connection.On<PlayerLeftGameMessage>(nameof(PlayerLeftGame), PlayerLeftGame);
        _connection.On<GameEndedMessage>(nameof(GameEnded), GameEnded);
        _connection.On<HandSetMessage>(nameof(HandSet), HandSet);
        _connection.On<string>(nameof(Error), Error);
        //_connection.Reconnected += (id) => { PlayerId = id!; return Task.CompletedTask; };
    }

    public async Task Join(Guid gameId, string name)
    {
        await _connection.StartAsync();
        PlayerId = _connection.ConnectionId!;
        Joined = await _connection.InvokeAsync<bool>("JoinGame", gameId, name);
    }

    public ValueTask DisposeAsync()
    {
        return _connection.State == HubConnectionState.Disconnected
            ? ValueTask.CompletedTask
            : _connection.DisposeAsync();
    }

    public Task CardsSelected(CardsSelectedMessage message)
    {
        StateChanged.Invoke();
        return Task.CompletedTask;
    }

    public async Task Error(string message)
    {
        ErrorMessage = message;
        await DisposeAsync();
    }

    public Task GameEnded(GameEndedMessage message)
    {
        throw new NotImplementedException();
    }

    public Task HandSet(HandSetMessage message)
    {
        Hand = message.WhiteCards;
        StateChanged.Invoke();
        return Task.CompletedTask;
    }

    public Task PlayerJoined(PlayerJoinedMessage message)
    {
        PlayerQueue = message.Players;
        StateChanged.Invoke();
        return Task.CompletedTask;
    }

    public Task PlayerLeftQueue(PlayerLeftQueueMessage message)
    {
        PlayerQueue = message.Players;
        StateChanged.Invoke();
        return Task.CompletedTask;
    }

    public Task PlayerLeftGame(PlayerLeftGameMessage message)
    {
        SelectedCards.Remove(message.PlayerId);
        Players = message.Players;
        StateChanged.Invoke();
        return Task.CompletedTask;
    }

    public Task RoundStarted(RoundStartedMessage message)
    {
        CardsRevealed = false;
        Waiting = false;
        BlackCard = message.BlackCard;
        CzarId = message.CzarId;
        Players = message.Players;
        PlayerQueue = Enumerable.Empty<PlayerInfo>();
        StateChanged.Invoke();
        return Task.CompletedTask;
    }

    public Task WaitingForPlayers(WaitingForPlayersMessage message)
    {
        Waiting = true;
        Players = Enumerable.Empty<PlayerInfo>();
        PlayerQueue = message.Players;
        StateChanged.Invoke();
        return Task.CompletedTask;
    }

    public Task RevealCards(RevealCardsMessage message)
    {
        CardsRevealed = true;
        StateChanged.Invoke();
        return Task.CompletedTask;
    }

    public Task WinnerSelected(WinnerSelectedMessage message)
    {
        WinningSet = new(message.PlayerName, message.BlackCard, message.WhiteCards);
        StateChanged.Invoke();
        return Task.CompletedTask;
    }
}
