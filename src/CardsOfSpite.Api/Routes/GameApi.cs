using CardsOfSpite.Api.Services;
using CardsOfSpite.GrainInterfaces;
using CardsOfSpite.GrainInterfaces.Models;
using CardsOfSpite.Models.Dto;
using Orleans;

namespace CardsOfSpite.Api.Routes;

public static class GameApi
{
    public static IEndpointRouteBuilder MapGameApi(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/game", CreateGame)
            .Produces<GameCreationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return builder;
    }

    static async Task<IResult> CreateGame(
        GameCreationRequest request,
        IClusterClient cluster,
        MessageStreamListener streamListener)
    {
        var gameId = Guid.NewGuid();
        var game = cluster.GetGrain<IGame>(gameId);

        var result = await game.Initialize(new GameConfiguration(
            request.MaxPlayers,
            request.MinPlayers,
            request.PointsToWin,
            10,
            request.AllowDiscard,
            request.DeckIds.ToHashSet()));

        await streamListener.RegisterGame(gameId);

        return result
            ? Results.Ok(new GameCreationResponse(gameId))
            : Results.BadRequest();
    }
}
