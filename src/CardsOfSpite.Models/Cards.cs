namespace CardsOfSpite.Models;

public record WhiteCard(Guid Id, string Text);
public record BlackCard(Guid Id, string Text, int Pick);