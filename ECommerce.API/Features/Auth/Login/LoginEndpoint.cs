using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Auth.Login
{
    [Route("auth")]
    public class LoginEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPost("login")]
        public async Task<IActionResult> Handle([FromBody] LoginRequest request)
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.Email == request.Email.ToLower());

            // Importante: devolvemos el mismo mensaje tanto si el email no existe
            // como si la contraseña es incorrecta. Esto es una práctica de seguridad
            // estándar — si diferenciáramos los dos casos, le estaríamos dando
            // información a un atacante sobre qué emails están registrados.
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Credenciales incorrectas" });

            // Creamos la sesión primero sin token para obtener el Id generado por la DB,
            // después generamos el JWT con ese Id, y finalmente actualizamos la sesión.
            // Este es el mismo patrón de dos pasos del proyecto Auth.
            var session = new Session
            {
                UserId = user.Id,
                Address = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Device = Request.Headers.UserAgent.FirstOrDefault() ?? "unknown",
                ExpiresAt = jwtService.GetExpiration()
            };

            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            var token = jwtService.GenerateToken(session.Id);
            session.Token = token;
            await db.SaveChangesAsync();

            return Ok(new LoginResponse(
                token,
                session.ExpiresAt,
                new UserDto(user.Id, user.Email, user.IsEmailVerified, user.IsAdmin)
            ));
        }
    }
}
