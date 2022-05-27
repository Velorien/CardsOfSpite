using CardsOfSpite.GrainInterfaces;
using CardsOfSpite.Grains.StateModels;
using CardsOfSpite.Models;
using Orleans;
using Orleans.Runtime;

namespace CardsOfSpite.Grains;

internal class DeckGrain : Grain, IDeck
{
    private readonly IPersistentState<DeckState> _state;

    public DeckGrain([PersistentState("deck")] IPersistentState<DeckState> state)
    {
        _state = state;
    }

    public async Task<Guid?> AddBlackCard(string text, int pick)
    {
        if (_state.State.Deck is null) return null;
        var cardId = Guid.NewGuid();
        _state.State.Deck.BlackCards.Add(new(cardId, text, pick));
        await _state.WriteStateAsync();
        return cardId;
    }

    public async Task<Guid?> AddWhiteCard(string text)
    {
        if (_state.State.Deck is null) return null;

        var cardId = Guid.NewGuid();
        _state.State.Deck.WhiteCards.Add(new(cardId, text));
        await _state.WriteStateAsync();
        return cardId;
    }

    public Task Cleanup() => _state.ClearStateAsync();

    public Task<Deck?> GetDeck() => Task.FromResult(_state.State.Deck);

    public Task<DeckInfo?> GetDeckInfo()
    {
        var deck = _state.State.Deck;
        if (deck is null) return Task.FromResult<DeckInfo?>(null);

        return Task.FromResult<DeckInfo?>(new DeckInfo(
            this.GetPrimaryKey(),
            deck.Name,
            deck.Group,
            deck.WhiteCards.Count,
            deck.BlackCards.Count));
    }

    public async Task<bool> Initialize(string name, string group)
    {
        if (_state.State.Deck is not null) return false;

        _state.State.Deck = new Deck(this.GetPrimaryKey(), name, group);
        await _state.WriteStateAsync();
        return true;
    }

    public async Task<bool> RemoveBlackCard(Guid cardId)
    {
        if (_state.State.Deck is null) return false;

        var card = _state.State.Deck.BlackCards.SingleOrDefault(x => x.Id == cardId);
        if (card is null) return false;
        _state.State.Deck.BlackCards.Remove(card);
        await _state.WriteStateAsync();
        return true;
    }

    public async Task<bool> RemoveWhiteCard(Guid cardId)
    {
        if (_state.State.Deck is null) return false;

        var card = _state.State.Deck.WhiteCards.SingleOrDefault(x => x.Id == cardId);
        if (card is null) return false;
        _state.State.Deck.WhiteCards.Remove(card);
        await _state.WriteStateAsync();
        return true;
    }

    public async Task<bool> Update(string name, string group)
    {
        if (_state.State.Deck is null) return false;

        _state.State.Deck = _state.State.Deck with { Name = name, Group = group };
        await _state.WriteStateAsync();
        return true;
    }
}
