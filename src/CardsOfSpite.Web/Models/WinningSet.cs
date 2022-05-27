using CardsOfSpite.Models;

namespace CardsOfSpite.Web.Models;

public record WinningSet(string PlayerName, BlackCard BlackCard, List<WhiteCard> WhiteCards);
