using System.ComponentModel.DataAnnotations;

namespace CardsOfSpite.Models.Dto;

public record DeckCreationRequest(
    [Required, MinLength(1)] string Name,
    [Required, MinLength(1)] string Group);

public record DeckUpdateRequest(
    [Required, MinLength(1)] string Name,
    [Required, MinLength(1)] string Group);

public record DeckCreationResponse(Guid DeckId, string Name, string Group);

public record WhiteCardCreationRequest(string Text);

public record WhiteCardCreationResponse(Guid Id, string Text);

public record BlackCardCreationRequest(string Text, int Pick);

public record BlackCardCreationResponse(Guid Id, string Text, int Pick);
