using CardsOfSpite.Models;

namespace CardsOfSpite.Grains.StateModels;
internal class Player : IEquatable<Player>
{
    public Player(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; }
    public string Name { get; }
    public List<WhiteCard> Hand { get; } = new();
    public int Points { get; set; }

    public bool Equals(Player? other) => other?.Id == Id;

    public override bool Equals(object? obj) => Equals(obj as Player);

    public override int GetHashCode() => Id.GetHashCode();
}
