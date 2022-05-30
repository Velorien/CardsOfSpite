using CardsOfSpite.GrainInterfaces;
using CardsOfSpite.Models;
using CardsOfSpite.Models.Dto;
using Orleans;

namespace CardsOfSpite.Api.Routes;

public static class DeckApi
{
    public static IEndpointRouteBuilder MapDeckApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/decks", GetDecks)
            .Produces<IEnumerable<DeckInfo>>();

        builder.MapPost("/api/decks", CreateDeck)
            .RequireAuthorization()
            .Produces<DeckCreationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        builder.MapDelete("/api/decks", DeleteDecks)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        builder.MapPut("/api/decks/{deckId:Guid}", UpdateDeck)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        builder.MapDelete("/api/decks/{deckId:Guid}", DeleteDeck)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        builder.MapPost("/api/decks/{deckId:Guid}/white", AddWhiteCard)
            .RequireAuthorization()
            .Produces<WhiteCardCreationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        builder.MapPost("/api/decks/{deckId:Guid}/black", AddBlackCard)
            .RequireAuthorization()
            .Produces<BlackCardCreationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
        
        builder.MapDelete("/api/decks/{deckId:Guid}/white/{cardId:Guid}", DeleteWhiteCard)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        builder.MapDelete("/api/decks/{deckId:Guid}/black/{cardId:Guid}", DeleteBlackCard)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return builder;
    }

    static async Task<IResult> GetDecks(IClusterClient cluster)
    {
        var deckRegistry = cluster.GetGrain<IDeckRegistry>(0);
        var deckIds = await deckRegistry.GetDeckIds();
        var deckInfos = await deckIds.ToAsyncEnumerable()
            .SelectAwait(async id => await cluster.GetGrain<IDeck>(id).GetDeckInfo())
            .ToListAsync();

        return Results.Ok(deckInfos);
    }

    static async Task<IResult> CreateDeck(DeckCreationRequest request, IClusterClient cluster)
    {
        var deckRegistry = cluster.GetGrain<IDeckRegistry>(0);
        var deckId = await deckRegistry.AddDeck(request.Name, request.Group);
        if (deckId is null) return Results.BadRequest();

        return Results.Ok(new DeckCreationResponse(deckId.Value, request.Name, request.Group));
    }

    static async Task<IResult> DeleteDecks(IClusterClient cluster)
    {
        var deckRegistry = cluster.GetGrain<IDeckRegistry>(0);
        var deckIds = await deckRegistry.GetDeckIds();
        foreach (var id in deckIds)
        {
            await deckRegistry.RemoveDeck(id);
        }

        return Results.NoContent();
    }

    static async Task<IResult> UpdateDeck(Guid deckId, DeckUpdateRequest request, IClusterClient cluster)
    {
        var deck = cluster.GetGrain<IDeck>(deckId);
        bool result = await deck.Update(request.Name, request.Group);
        return result
            ? Results.NoContent()
            : Results.BadRequest();
    }

    static async Task<IResult> DeleteDeck(Guid deckId, IClusterClient cluster)
    {
        var deckRegistry = cluster.GetGrain<IDeckRegistry>(0);
        await deckRegistry.RemoveDeck(deckId);
        return Results.NoContent();
    }

    static async Task<IResult> AddWhiteCard(Guid deckId, WhiteCardCreationRequest request, IClusterClient cluster)
    {
        var deck = cluster.GetGrain<IDeck>(deckId);
        var cardId = await deck.AddWhiteCard(request.Text);
        if (cardId is null) return Results.BadRequest();

        return Results.Ok(new WhiteCardCreationResponse(cardId.Value, request.Text));
    }

    static async Task<IResult> AddBlackCard(Guid deckId, BlackCardCreationRequest request, IClusterClient cluster)
    {
        var deck = cluster.GetGrain<IDeck>(deckId);
        var cardId = await deck.AddBlackCard(request.Text, request.Pick);
        if (cardId is null) return Results.BadRequest();

        return Results.Ok(new BlackCardCreationResponse(cardId.Value, request.Text, request.Pick));
    }

    static async Task<IResult> DeleteWhiteCard(Guid deckId, Guid cardId, IClusterClient cluster)
    {
        var deck = cluster.GetGrain<IDeck>(deckId);
        bool result = await deck.RemoveWhiteCard(cardId);
        return result
            ? Results.NoContent()
            : Results.BadRequest();
    }

    static async Task<IResult> DeleteBlackCard(Guid deckId, Guid cardId, IClusterClient cluster)
    {
        var deck = cluster.GetGrain<IDeck>(deckId);
        bool result = await deck.RemoveBlackCard(cardId);
        return result
            ? Results.NoContent()
            : Results.BadRequest();
    }
}
