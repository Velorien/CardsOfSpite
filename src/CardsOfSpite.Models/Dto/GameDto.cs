namespace CardsOfSpite.Models.Dto;
public record GameCreationRequest(
    IEnumerable<Guid> DeckIds,
    int MinPlayers,
    int MaxPlayers,
    int PointsToWin,
    bool AllowDiscard);

public record GameCreationResponse(Guid GameId);