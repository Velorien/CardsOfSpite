namespace CardsOfSpite.Models;

public record Deck(Guid Id, string Name, string Group)
{
    public HashSet<WhiteCard> WhiteCards { get; init; } = new();
    public HashSet<BlackCard> BlackCards { get; init; } = new();
}
