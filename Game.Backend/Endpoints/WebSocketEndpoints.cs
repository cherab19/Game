using Game.Backend.Contracts;
using Game.Backend.Data;
using Game.Backend.Security;
using Game.Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Endpoints;

public static class WebSocketEndpoints
{
    public static IEndpointRouteBuilder MapWebSocketEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/ws/game", HandleAsync)
            .WithTags("Realtime")
            .RequireAuthorization();

        return app;
    }

    private static async Task HandleAsync(
        HttpContext httpContext,
        GameDbContext dbContext,
        GameSocketHub socketHub,
        CancellationToken cancellationToken)
    {
        if (!httpContext.WebSockets.IsWebSocketRequest)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object>.Fail("websocket_required", "This endpoint only accepts WebSocket connections."),
                cancellationToken);
            return;
        }

        var playerId = httpContext.User.GetRequiredPlayerId();
        var player = await dbContext.Players.AsNoTracking().FirstOrDefaultAsync(entry => entry.Id == playerId, cancellationToken);

        if (player is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponse<object>.Fail("player_not_found", "Player profile was not found."),
                cancellationToken);
            return;
        }

        Guid? lobbyId = null;
        if (Guid.TryParse(httpContext.Request.Query["lobbyId"], out var requestedLobbyId))
        {
            var hasLobbyAccess = await dbContext.LobbyMembers.AnyAsync(
                member => member.LobbyId == requestedLobbyId && member.PlayerId == playerId,
                cancellationToken);

            if (!hasLobbyAccess)
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("lobby_forbidden", "Player is not a member of the requested lobby."),
                    cancellationToken);
                return;
            }

            lobbyId = requestedLobbyId;
        }

        using var socket = await httpContext.WebSockets.AcceptWebSocketAsync();
        await socketHub.RunSessionAsync(player.Id, player.Username, lobbyId, socket, cancellationToken);
    }
}