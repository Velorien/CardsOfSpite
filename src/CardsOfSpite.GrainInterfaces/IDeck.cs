using CardsOfSpite.Models;
using Orleans;

namespace CardsOfSpite.GrainInterfaces;

public interface IDeck : IGrainWithGuidKey
{
    Task<DeckInfo?> GetDeckInfo();
    Task<Deck?> GetDeck();
    Task<bool> Initialize(string name, string group);
    Task<bool> Update(string name, string group);
    Task<Guid?> AddWhiteCard(string text);
    Task<bool> RemoveWhiteCard(Guid cardId);
    Task<Guid?> AddBlackCard(string text, int pick);
    Task<bool> RemoveBlackCard(Guid cardId);
    Task Cleanup();
}
