namespace CardsOfSpite.Grains.StateModels;
internal class DeckRegistryState
{
    public HashSet<Guid> DeckIds { get; set; } = new();
}
