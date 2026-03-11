namespace Game.Backend.Contracts;

public sealed record RegisterRequest(string Username, string Email, string Password, string? DisplayName);

public sealed record LoginRequest(string UsernameOrEmail, string Password);

public sealed record AuthResponse(string Token, DateTime ExpiresAtUtc, PlayerProfileResponse Player);