using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Models;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Auth.Register
{
    [Route("auth")]
    public class RegisterEndpoint(AppDbContext db, IJwtService jwtService, IEmailService emailService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPost("register")]
        public async Task<IActionResult> Handle([FromBody] RegisterRequest request)
        {
            // Verificamos si el email ya está en uso antes de crear nada.
            // Hacemos la comparación case-insensitive porque "Test@test.com"
            // y "test@test.com" deben tratarse como el mismo email.
            var exists = await db.Users.AnyAsync(u =>
                u.Email.ToLower() == request.Email.ToLower());

            if (exists)
                return Conflict(new { message = "Ya existe una cuenta con ese email" });

            // Generamos un código de 6 dígitos para verificar el email.
            // En producción este código se envía por email — en desarrollo
            // el código "123456" siempre funciona como bypass.
            var verificationCode = Random.Shared.Next(100000, 999999).ToString();

            var user = new User
            {
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 10),
                EmailVerificationCode = verificationCode
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Enviamos el email con el código. Si falla (por ejemplo en desarrollo
            // sin servidor SMTP configurado), el EmailService loguea el error
            // pero no interrumpe el flujo — el usuario igual puede usar "123456".
            await emailService.SendEmailAsync(
                user.Email,
                "Verificá tu cuenta",
                $"Tu código de verificación es: <strong>{verificationCode}</strong>"
            );

            return CreatedAtAction(
                nameof(Handle),
                new { id = user.Id },
                new RegisterResponse(user.Id, user.Email, user.IsEmailVerified, user.CreatedAt)
            );
        }
    }
}
