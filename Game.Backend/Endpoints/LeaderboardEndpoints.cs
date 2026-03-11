using Game.Backend.Contracts;
using Game.Backend.Data;
using Game.Backend.Entities;
using Game.Backend.Security;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Endpoints;

public static class LeaderboardEndpoints
{
    public static IEndpointRouteBuilder MapLeaderboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/leaderboards").WithTags("Leaderboards");

        group.MapGet("/top", GetTopAsync);
        group.MapPost("/update", UpdateAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetTopAsync(GameDbContext dbContext, int? limit, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(limit ?? 10, 1, 100);

        var leaderboard = await dbContext.Leaderboards
            .AsNoTracking()
            .Include(entry => entry.Player)
            .OrderByDescending(entry => entry.Score)
            .ThenByDescending(entry => entry.Wins)
            .ThenBy(entry => entry.Losses)
            .ThenByDescending(entry => entry.Kills)
            .Take(take)
            .ToListAsync(cancellationToken);

        var response = leaderboard.Select(entry => entry.ToResponse()).ToList();
        return Results.Ok(ApiResponse<IReadOnlyList<LeaderboardEntryResponse>>.Ok(response));
    }

    private static async Task<IResult> UpdateAsync(
        HttpContext httpContext,
        LeaderboardUpdateRequest request,
        GameDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var playerId = httpContext.User.GetRequiredPlayerId();
        var entry = await dbContext.Leaderboards
            .Include(leaderboardEntry => leaderboardEntry.Player)
            .FirstOrDefaultAsync(leaderboardEntry => leaderboardEntry.PlayerId == playerId, cancellationToken);

        if (entry is null)
        {
            var player = await dbContext.Players.FirstOrDefaultAsync(existingPlayer => existingPlayer.Id == playerId, cancellationToken);

            if (player is null)
            {
                return Results.NotFound(ApiResponse<object>.Fail("player_not_found", "Player profile was not found."));
            }

            entry = new LeaderboardEntry { PlayerId = playerId, Player = player };
            dbContext.Leaderboards.Add(entry);
        }

        entry.Score = Math.Max(0, entry.Score + request.ScoreDelta);
        entry.Wins = Math.Max(0, entry.Wins + request.WinsDelta);
        entry.Losses = Math.Max(0, entry.Losses + request.LossesDelta);
        entry.Kills = Math.Max(0, entry.Kills + request.KillsDelta);
        entry.Deaths = Math.Max(0, entry.Deaths + request.DeathsDelta);
        entry.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(ApiResponse<LeaderboardEntryResponse>.Ok(entry.ToResponse()));
    }
}