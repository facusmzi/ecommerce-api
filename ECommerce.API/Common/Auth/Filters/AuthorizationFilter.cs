using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Common.Auth.Filters
{
    // Marca un endpoint como accesible solo por usuarios con email verificado.
    // Se usa junto con [Authorize] que verifica que el JWT sea válido.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireVerifiedEmailAttribute : Attribute { }

    // Marca un endpoint como accesible solo por administradores.
    // Equivale al [RequireCreator] del proyecto Auth pero renombrado
    // para que tenga más sentido en el contexto de un e-commerce.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireAdminAttribute : Attribute { }

    public class AuthorizationFilter(AppDbContext db, IJwtService jwtService) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var requiresVerifiedEmail = context.ActionDescriptor.EndpointMetadata
                .OfType<RequireVerifiedEmailAttribute>().Any();

            var requiresAdmin = context.ActionDescriptor.EndpointMetadata
                .OfType<RequireAdminAttribute>().Any();

            // Si el endpoint no tiene ninguno de nuestros atributos custom,
            // no hacemos nada y dejamos pasar el request.
            if (!requiresVerifiedEmail && !requiresAdmin)
            {
                await next();
                return;
            }

            // Resolvemos el usuario desde el JWT, igual que en BaseEndpoint.
            // Necesitamos hacerlo acá también porque el filter corre antes
            // de que el controller llegue a ejecutarse.
            var authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader is null || !authHeader.StartsWith("Bearer "))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var token = authHeader["Bearer ".Length..];
            var sessionId = jwtService.ValidateToken(token);
            if (sessionId is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var session = await db.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.ExpiresAt > DateTime.UtcNow);

            if (session is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var user = session.User;

            if (requiresVerifiedEmail && !user.IsEmailVerified)
            {
                context.Result = new ObjectResult(new { message = "Email no verificado" })
                {
                    StatusCode = 403
                };
                return;
            }

            if (requiresAdmin && !user.IsAdmin)
            {
                context.Result = new ObjectResult(new { message = "Se requieren permisos de administrador" })
                {
                    StatusCode = 403
                };
                return;
            }

            await next();
        }
    }
}
