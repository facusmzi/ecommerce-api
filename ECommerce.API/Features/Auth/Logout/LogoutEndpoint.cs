using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Auth.Logout
{
    [Route("auth")]
    public class LogoutEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Handle()
        {
            var token = GetCurrentToken();

            // Borramos la sesión de la base de datos.
            // Esto es lo que hace que el sistema de sesiones sea superior
            // a un JWT puro: aunque el token siga siendo criptográficamente
            // válido por días, sin la sesión en la DB el BaseEndpoint va a
            // devolver null en GetCurrentUserAsync() y el usuario quedará
            // efectivamente deslogueado.
            var session = await db.Sessions.FirstOrDefaultAsync(s => s.Token == token);
            if (session is not null)
            {
                db.Sessions.Remove(session);
                await db.SaveChangesAsync();
            }

            return Ok(new { message = "Sesión cerrada correctamente" });
        }
    }
}
