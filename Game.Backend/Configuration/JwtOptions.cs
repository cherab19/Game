namespace Game.Backend.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "Game.Backend";

    public string Audience { get; init; } = "Game.UnityClient";

    public string Key { get; init; } = "ChangeThisSuperSecretKeyBeforeProductionUse123!";

    public int ExpiryMinutes { get; init; } = 120;
}