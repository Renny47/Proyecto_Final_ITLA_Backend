namespace SIGID.Infrastructure.Security;

public class JwtSettings
{
    public string SecretKey { get; set; } = "SuperSecretKeyForSIGIDJwtTokensWithMinimum256Bits!";
    public string Issuer { get; set; } = "SIGID";
    public string Audience { get; set; } = "SIGID-Users";
    public int TokenExpirationHours { get; set; } = 24;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}