namespace FinSenseAPI.Config;

public class JwtOptions
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpiryDays { get; set; } = 7;
    public int RefreshTokenExpiryDays { get; set; } = 30;
}
