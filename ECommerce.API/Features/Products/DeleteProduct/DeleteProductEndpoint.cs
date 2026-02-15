using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Features.Products.DeleteProduct
{
    [Route("products")]
    public class DeleteProductEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpDelete("{id:guid}")]
        [Authorize]
        [RequireVerifiedEmail]
        [RequireAdmin]
        public async Task<IActionResult> Handle(Guid id)
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
                return NotFound(new { message = "Producto no encontrado" });

            // En lugar de borrar físicamente el registro, lo "desactivamos".
            // Esto se llama soft delete y es una práctica muy común en e-commerce
            // porque los productos borrados pueden seguir siendo referenciados
            // en órdenes históricas — si los borráramos físicamente, esas
            // órdenes perderían información sobre qué se compró.
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            // 204 No Content es la respuesta estándar para un delete exitoso —
            // no hay nada que devolver porque el recurso ya no está activo.
            return NoContent();
        }
    }
}
