using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Auth.VerifyEmail
{
    [Route("auth")]
    public class VerifyEmailEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPost("verify-email")]
        public async Task<IActionResult> Handle([FromBody] VerifyEmailRequest request)
        {
            var user = await db.Users.FirstOrDefaultAsync(u =>
                u.Email == request.Email.ToLower());

            if (user is null)
                return NotFound(new { message = "Usuario no encontrado" });

            if (user.IsEmailVerified)
                return BadRequest(new { message = "El email ya fue verificado" });

            // El bypass de desarrollo: el código "123456" siempre funciona.
            // Esto es idéntico al comportamiento del proyecto Auth original
            // y nos permite testear sin necesitar un servidor SMTP real.
            var isValidCode = request.Code == "123456" || request.Code == user.EmailVerificationCode;

            if (!isValidCode)
                return BadRequest(new { message = "Código de verificación incorrecto" });

            user.IsEmailVerified = true;
            user.EmailVerificationCode = null; // limpiamos el código para que no se pueda reusar
            user.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Ok(new { message = "Email verificado correctamente" });
        }
    }
}
