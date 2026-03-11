using Game.Backend.Contracts;
using Game.Backend.Data;
using Game.Backend.Entities;
using Game.Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        GameDbContext dbContext,
        JwtTokenService tokenService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(ApiResponse<object>.Fail("validation_error", "Username, email, and password are required."));
        }

        if (request.Password.Length < 8)
        {
            return Results.BadRequest(ApiResponse<object>.Fail("weak_password", "Password must be at least 8 characters long."));
        }

        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var usernameNormalized = username.ToLowerInvariant();
        var emailNormalized = email.ToLowerInvariant();

        var playerExists = await dbContext.Players.AnyAsync(
            player => player.UsernameNormalized == usernameNormalized || player.EmailNormalized == emailNormalized,
            cancellationToken);

        if (playerExists)
        {
            return Results.Conflict(ApiResponse<object>.Fail("player_exists", "A player with that username or email already exists."));
        }

        var player = new Player
        {
            Username = username,
            UsernameNormalized = usernameNormalized,
            Email = email,
            EmailNormalized = emailNormalized,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? username : request.DisplayName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        dbContext.Players.Add(player);
        dbContext.Leaderboards.Add(new LeaderboardEntry { Player = player });
        await dbContext.SaveChangesAsync(cancellationToken);

        var token = tokenService.CreateToken(player);
        var response = new AuthResponse(token.Token, token.ExpiresAtUtc, player.ToProfileResponse());

        return Results.Ok(ApiResponse<AuthResponse>.Ok(response));
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        GameDbContext dbContext,
        JwtTokenService tokenService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(ApiResponse<object>.Fail("validation_error", "Username/email and password are required."));
        }

        var login = request.UsernameOrEmail.Trim().ToLowerInvariant();

        var player = await dbContext.Players.FirstOrDefaultAsync(
            entry => entry.UsernameNormalized == login || entry.EmailNormalized == login,
            cancellationToken);

        if (player is null || !BCrypt.Net.BCrypt.Verify(request.Password, player.PasswordHash))
        {
            return Results.Json(
                ApiResponse<object>.Fail("invalid_credentials", "Username/email or password is incorrect."),
                statusCode: StatusCodes.Status401Unauthorized);
        }

        player.LastLoginAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var token = tokenService.CreateToken(player);
        var response = new AuthResponse(token.Token, token.ExpiresAtUtc, player.ToProfileResponse());

        return Results.Ok(ApiResponse<AuthResponse>.Ok(response));
    }
}