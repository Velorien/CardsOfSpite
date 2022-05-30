namespace CardsOfSpite.Models.Messages;

public abstract record Message(Guid GameId);

public abstract record TargetedMessage(Guid GameId, string PlayerId) : Message(GameId);

public record PlayerJoinedMessage(Guid GameId, List<PlayerInfo> Players) : Message(GameId);

public record PlayerLeftGameMessage(Guid GameId, string PlayerId, List<PlayerInfo> Players) : Message(GameId);

public record PlayerLeftQueueMessage(Guid GameId, List<PlayerInfo> Players) : Message(GameId);

public record CardsSelectedMessage(Guid GameId, string PlayerId, List<WhiteCard> Cards) : Message(GameId);

public record RevealCardsMessage(Guid GameId) : Message(GameId);

public record WinnerSelectedMessage(
    Guid GameId,
    string PlayerName,
    BlackCard BlackCard,
    List<WhiteCard> WhiteCards) : Message(GameId);

public record GameEndedMessage(
    Guid GameId,
    string Name,
    BlackCard BlackCard,
    List<WhiteCard> WhiteCards) : Message(GameId);

public record HandSetMessage(
    Guid GameId,
    string PlayerId,
    List<WhiteCard> WhiteCards) : TargetedMessage(GameId, PlayerId);

public record RoundStartedMessage(
    Guid GameId,
    string CzarId,
    BlackCard BlackCard,
    List<PlayerInfo> Players) : Message(GameId);

public record WaitingForPlayersMessage(Guid GameId, List<PlayerInfo> Players) : Message(GameId);