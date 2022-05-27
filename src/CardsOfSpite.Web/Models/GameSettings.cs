using System.ComponentModel.DataAnnotations;

namespace CardsOfSpite.Web.Models;

public class GameSettings
{
    public HashSet<Guid> DeckIds { get; } = new();
    public bool AllowDiscard { get; set; }
    [Range(3, 20)] public int MinPlayers { get; set; } = 3;
    [Range(3, 20)] public int MaxPlayers { get; set; } = 10;
    [Range(1, 100)] public int PointsToWin { get; set; } = 10;
}
