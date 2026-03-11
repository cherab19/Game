namespace Game.Backend.Contracts;

public sealed record PlayerProfileResponse(
    Guid Id,
    string Username,
    string DisplayName,
    string Email,
    string? AvatarUrl,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc);

public sealed record UpdatePlayerProfileRequest(string? DisplayName, string? AvatarUrl);