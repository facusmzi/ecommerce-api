using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Common.Auth
{
    // Todos los controllers del proyecto van a heredar de esta clase
    // en lugar de heredar directamente de ControllerBase.
    // Esto nos da acceso a GetCurrentUserAsync() en cualquier endpoint
    // sin repetir la lógica de resolución JWT → Session → User.
    [ApiController]
    public abstract class BaseEndpoint(AppDbContext db, IJwtService jwtService) : ControllerBase
    {
        // Resuelve el usuario actual a partir del JWT en el header Authorization.
        // Devuelve null si el token no existe, es inválido, o la sesión fue eliminada.
        // La cadena es: JWT → sessionId → Session en DB → User en DB.
        protected async Task<User?> GetCurrentUserAsync()
        {
            var token = GetCurrentToken();
            if (token is null) return null;

            var sessionId = jwtService.ValidateToken(token);
            if (sessionId is null) return null;

            var session = await db.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.ExpiresAt > DateTime.UtcNow);

            return session?.User;
        }

        // Extrae el token JWT crudo del header Authorization.
        // El header viene con el formato "Bearer eyJhbG..." así que
        // necesitamos sacar el prefijo "Bearer " para quedarnos solo con el token.
        protected string? GetCurrentToken()
        {
            var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader is null || !authHeader.StartsWith("Bearer ")) return null;
            return authHeader["Bearer ".Length..];
        }
    }
}
