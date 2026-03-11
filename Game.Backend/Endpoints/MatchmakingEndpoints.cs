using Game.Backend.Contracts;
using Game.Backend.Security;
using Game.Backend.Services;

namespace Game.Backend.Endpoints;

public static class MatchmakingEndpoints
{
    public static IEndpointRouteBuilder MapMatchmakingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/matchmaking").WithTags("Matchmaking").RequireAuthorization();

        group.MapPost("/join", JoinAsync);
        group.MapPost("/start", StartAsync);

        return app;
    }

    private static async Task<IResult> JoinAsync(
        HttpContext httpContext,
        JoinMatchmakingRequest request,
        MatchmakingService matchmakingService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await matchmakingService.JoinAsync(httpContext.User.GetRequiredPlayerId(), request, cancellationToken);
            var response = new JoinMatchmakingResponse(result.Created, result.AlreadyJoined, result.Lobby.ToResponse());
            return Results.Ok(ApiResponse<JoinMatchmakingResponse>.Ok(response));
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(ApiResponse<object>.Fail("matchmaking_error", exception.Message));
        }
    }

    private static async Task<IResult> StartAsync(
        HttpContext httpContext,
        StartMatchRequest request,
        MatchmakingService matchmakingService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await matchmakingService.StartAsync(httpContext.User.GetRequiredPlayerId(), request.LobbyId, cancellationToken);
            var response = new MatchStartResponse(result.Game.Id, result.Game.Status, result.Game.StartedAtUtc, result.Lobby.ToResponse());
            return Results.Ok(ApiResponse<MatchStartResponse>.Ok(response));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Results.Json(ApiResponse<object>.Fail("forbidden", exception.Message), statusCode: StatusCodes.Status403Forbidden);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(ApiResponse<object>.Fail("matchmaking_error", exception.Message));
        }
    }
}