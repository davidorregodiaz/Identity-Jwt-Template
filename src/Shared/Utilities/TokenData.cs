namespace Shared.Utilities;

public class TokenData
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
    public bool IsExpired =>  Expiration < DateTime.UtcNow;
}