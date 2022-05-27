using CardsOfSpite.Models.Messages;

namespace CardsOfSpite.HubClient;

public interface IGameHubClient
{
    Task Error(string message);
    Task PlayerJoined(PlayerJoinedMessage message);
    Task PlayerLeftGame(PlayerLeftGameMessage message);
    Task PlayerLeftQueue(PlayerLeftQueueMessage message);
    Task CardsSelected(CardsSelectedMessage message);
    Task RevealCards(RevealCardsMessage message);
    Task WinnerSelected(WinnerSelectedMessage message);
    Task GameEnded(GameEndedMessage message);
    Task HandSet(HandSetMessage message);
    Task RoundStarted(RoundStartedMessage message);
    Task WaitingForPlayers(WaitingForPlayersMessage message);
}
