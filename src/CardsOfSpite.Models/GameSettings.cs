namespace CardsOfSpite.Models;
public record GameSettings(
    int MaxPlayers,
    int MinPlayers,
    int PointsToWin,
    int HandSize,
    bool AllowDiscard,
    HashSet<Guid> DeckIds);
