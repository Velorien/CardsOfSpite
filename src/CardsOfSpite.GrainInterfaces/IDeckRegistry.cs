using Orleans;

namespace CardsOfSpite.GrainInterfaces;
public interface IDeckRegistry : IGrainWithIntegerKey
{
    public Task<Guid?> AddDeck(string name, string group);
    public Task<bool> RemoveDeck(Guid deckId);
    public Task<IEnumerable<Guid>> GetDeckIds();
}
