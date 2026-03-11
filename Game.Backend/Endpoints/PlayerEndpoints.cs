using Game.Backend.Contracts;
using Game.Backend.Data;
using Game.Backend.Security;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Endpoints;

public static class PlayerEndpoints
{
    public static IEndpointRouteBuilder MapPlayerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/players").WithTags("Players").RequireAuthorization();

        group.MapGet("/me", GetMeAsync);
        group.MapPut("/me", UpdateMeAsync);

        return app;
    }

    private static async Task<IResult> GetMeAsync(HttpContext httpContext, GameDbContext dbContext, CancellationToken cancellationToken)
    {
        var playerId = httpContext.User.GetRequiredPlayerId();
        var player = await dbContext.Players.AsNoTracking().FirstOrDefaultAsync(entry => entry.Id == playerId, cancellationToken);

        if (player is null)
        {
            return Results.NotFound(ApiResponse<object>.Fail("player_not_found", "Player profile was not found."));
        }

        return Results.Ok(ApiResponse<PlayerProfileResponse>.Ok(player.ToProfileResponse()));
    }

    private static async Task<IResult> UpdateMeAsync(
        HttpContext httpContext,
        UpdatePlayerProfileRequest request,
        GameDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var playerId = httpContext.User.GetRequiredPlayerId();
        var player = await dbContext.Players.FirstOrDefaultAsync(entry => entry.Id == playerId, cancellationToken);

        if (player is null)
        {
            return Results.NotFound(ApiResponse<object>.Fail("player_not_found", "Player profile was not found."));
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            player.DisplayName = request.DisplayName.Trim();
        }

        player.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<PlayerProfileResponse>.Ok(player.ToProfileResponse()));
    }
}