namespace ECommerce.API.Common.Services
{
    public interface IJwtService
    {
        string GenerateToken(Guid sessionId);
        Guid? ValidateToken(string token);
        DateTime GetExpiration();
    }
}
