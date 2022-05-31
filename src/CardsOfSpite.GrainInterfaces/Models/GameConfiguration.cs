namespace CardsOfSpite.GrainInterfaces.Models;

public record GameConfiguration(
    int MaxPlayers,
    int MinPlayers,
    int PointsToWin,
    int HandSize,
    bool AllowDiscard,
    HashSet<Guid> DeckIds);
