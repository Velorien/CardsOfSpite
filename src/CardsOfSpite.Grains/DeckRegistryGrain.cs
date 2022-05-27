using CardsOfSpite.GrainInterfaces;
using CardsOfSpite.Grains.StateModels;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsOfSpite.Grains;
internal class DeckRegistryGrain : Grain, IDeckRegistry
{
    private readonly IPersistentState<DeckRegistryState> _state;

    public DeckRegistryGrain(
        [PersistentState("deckRegistry")] IPersistentState<DeckRegistryState> state)
    {
        _state = state;
    }

    public async Task<Guid?> AddDeck(string name, string group)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(group))
        {
            return null;
        }

        var deckId = Guid.NewGuid();
        var deck = GrainFactory.GetGrain<IDeck>(deckId);
        bool result = await deck.Initialize(name, group);
        if (!result) return null;

        _state.State.DeckIds.Add(deckId);
        await _state.WriteStateAsync();

        return deckId;
    }

    public Task<IEnumerable<Guid>> GetDeckIds() => Task.FromResult<IEnumerable<Guid>>(_state.State.DeckIds);

    public async Task<bool> RemoveDeck(Guid deckId)
    {
        var deck = GrainFactory.GetGrain<IDeck>(deckId);
        await deck.Cleanup();
        if (!_state.State.DeckIds.Contains(deckId)) return false;
        _state.State.DeckIds.Remove(deckId);
        await _state.WriteStateAsync();
        return true;
    }
}
