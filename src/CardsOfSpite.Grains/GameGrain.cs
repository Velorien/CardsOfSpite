using CardsOfSpite.GrainInterfaces;
using CardsOfSpite.Grains.StateModels;
using CardsOfSpite.Models;
using CardsOfSpite.Models.Messages;
using Orleans;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsOfSpite.Grains;
internal class GameGrain : Grain, IGame
{
    private readonly Dictionary<string, Player> _players = new();
    private readonly List<Player> _playerQueue = new();
    private readonly List<WhiteCard> _whiteCards = new();
    private readonly List<BlackCard> _blackCards = new();
    private readonly List<WhiteCard> _whiteDiscardPile = new();
    private readonly List<BlackCard> _blackDiscardPile = new();
    private readonly List<string> _czarQueue = new();
    private readonly Dictionary<string, List<WhiteCard>> _selectedCards = new();

    private Guid _gameId;
    private string? _czarId;
    private BlackCard? _currentBlackCard;
    private GameSettings? _settings;
    private IAsyncStream<Message> _messageStream = null!;

    public override Task OnActivateAsync()
    {
        _gameId = this.GetPrimaryKey();
        return Task.CompletedTask;
    }

    public async Task DiscardHand(string playerId)
    {
        if (_settings is null) return;

        if (_settings.AllowDiscard &&
            !_selectedCards.ContainsKey(playerId) &&
            _players.TryGetValue(playerId, out var player) &&
            player.Points > 0)
        {
            _whiteDiscardPile.AddRange(player.Hand);
            player.Hand.Clear();
            player.Hand.AddRange(DrawWhiteCards(_settings.HandSize));
            await SendMessage(new HandSetMessage(_gameId, playerId, player.Hand));
        }
    }

    public async Task<bool> Initialize(GameSettings settings)
    {
        var deckGrains = settings.DeckIds.Select(id => GrainFactory.GetGrain<IDeck>(id)).ToList();
        var decks = await deckGrains
            .ToAsyncEnumerable()
            .SelectAwait(async deck => await deck.GetDeck())
            .Where(x => x is not null)
            .ToListAsync();

        _whiteCards.AddRange(decks.SelectMany(deck => deck!.WhiteCards).OrderBy(x => Random.Shared.Next()));
        _blackCards.AddRange(decks.SelectMany(deck => deck!.BlackCards).OrderBy(x => Random.Shared.Next()));

        if (_whiteCards.Count < settings.MaxPlayers * settings.HandSize) return false;

        _messageStream = GetStreamProvider("SMS").GetStream<Message>(this.GetPrimaryKey(), null);
        _settings = settings;
        return true;
    }

    public async Task<bool> Join(string playerId, string name)
    {
        if (_settings is null ||
            _playerQueue.Any(x => x.Id == playerId) ||
            _players.ContainsKey(playerId))
        {
            return false;
        }

        _playerQueue.Add(new Player(playerId, name));
        await SendMessage(new PlayerJoinedMessage(_gameId, GetPlayerQueueInfo()));

        // start new round when the min number of players is reached
        if (!_players.Any() && _playerQueue.Count == _settings!.MinPlayers)
        {
            await StartNextRound();
        }

        return true;
    }

    public async Task Leave(string playerId)
    {
        if (_settings is null) return;

        if (_players.TryGetValue(playerId, out var player))
        {
            _whiteDiscardPile.AddRange(player.Hand);
            _czarQueue.Remove(playerId);
            _players.Remove(playerId);
            await SendMessage(new PlayerLeftGameMessage(_gameId, playerId, GetPlayerInfo()));

            if (playerId != _czarId)
            {
                if (_selectedCards.TryGetValue(playerId, out var selectedCards))
                {
                    _whiteDiscardPile.AddRange(selectedCards);
                    _selectedCards.Remove(playerId);
                }
                else if (_selectedCards.Count == _players.Count - 1)
                {
                    await SendMessage(new RevealCardsMessage(_gameId));
                }
            }
        }

        player = _playerQueue.FirstOrDefault(x => x.Id == playerId);
        if (player is not null)
        {
            _playerQueue.Remove(player);
            await SendMessage(new PlayerLeftQueueMessage(_gameId, GetPlayerQueueInfo()));
        }

        if (_players.Count < _settings.MinPlayers || playerId == _czarId)
        {
            await StartNextRound();
        }
    }

    public async Task<bool> SelectCards(string playerId, List<Guid> cardIds)
    {
        if (cardIds.Count != _currentBlackCard!.Pick) return false;
        _selectedCards[playerId] = new();
        foreach (var id in cardIds)
        {
            var card = _players[playerId].Hand.First(card => card.Id == id);
            _players[playerId].Hand.Remove(card);
            _selectedCards[playerId].Add(card);
        }

        await SendMessage(new CardsSelectedMessage(_gameId, playerId, _selectedCards[playerId]));
        if (_selectedCards.Count == _players.Count - 1)
            await SendMessage(new RevealCardsMessage(_gameId));

        return true;
    }

    public async Task SelectWinner(string playerId)
    {
        if (!_players.ContainsKey(playerId)) return;

        _players[playerId].Points++;
        if (_players[playerId].Points == _settings!.PointsToWin)
        {
            await SendMessage(new GameEndedMessage(
                _gameId,
                _players[playerId].Name,
                _currentBlackCard!,
                _selectedCards[playerId]));
            return;
        }

        await SendMessage(new WinnerSelectedMessage(_gameId, _players[playerId].Name, _currentBlackCard!, _selectedCards[playerId]));

        _blackDiscardPile.Add(_currentBlackCard!);
        foreach (var selection in _selectedCards)
        {
            _whiteDiscardPile.AddRange(selection.Value);
        }

        await StartNextRound();
    }

    private async Task StartNextRound()
    {
        _selectedCards.Clear();

        int czarIndex = _czarId is null ? 0 : _czarQueue.IndexOf(_czarId);
        foreach (var player in _playerQueue)
        {
            _players[player.Id] = player;
            _czarQueue.Insert(Math.Max(0, czarIndex), player.Id);
        }

        _playerQueue.Clear();

        if (_players.Count < _settings!.MinPlayers)
        {
            _playerQueue.AddRange(_players.Values);
            _players.Clear();
            await SendMessage(new WaitingForPlayersMessage(_gameId, GetPlayerQueueInfo()));
            return;
        }

        foreach (var player in _players.Values)
        {
            player.Hand.AddRange(DrawWhiteCards(_settings!.HandSize - player.Hand.Count));
            await SendMessage(new HandSetMessage(_gameId, player.Id, player.Hand));
        }

        // select next czar
        _czarId = _czarId is null
            ? _players.Keys.First()
            : _czarQueue[(_czarQueue.IndexOf(_czarId) + 1) % _czarQueue.Count];

        DrawBlackCard();
        await SendMessage(new RoundStartedMessage(
            _gameId,
            _czarId,
            _currentBlackCard!,
            GetPlayerInfo()));
    }

    private IEnumerable<WhiteCard> DrawWhiteCards(int count)
    {
        if (_whiteCards.Count < count)
        {
            _whiteCards.AddRange(_whiteDiscardPile.OrderBy(_ => Random.Shared.Next()));
            _whiteDiscardPile.Clear();
        }

        var draw = _whiteCards.Take(count).ToList();
        _whiteCards.RemoveRange(0, count);

        return draw;
    }

    private void DrawBlackCard()
    {
        if (!_blackCards.Any())
        {
            _blackCards.AddRange(_blackDiscardPile.OrderBy(_ => Random.Shared.Next()));
        }

        _currentBlackCard = _blackCards.First();
        _blackCards.RemoveAt(0);
    }

    private Task SendMessage(Message message) => _messageStream.OnNextAsync(message);

    private List<PlayerInfo> GetPlayerQueueInfo() =>
        _playerQueue.Select(p => new PlayerInfo(p.Id, p.Name, p.Points)).ToList();

    private List<PlayerInfo> GetPlayerInfo() =>
        _players.Values.Select(p => new PlayerInfo(p.Id, p.Name, p.Points)).ToList();
}
