using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Products.UpdateProduct
{
    [Route("products")]
    public class UpdateProductEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPut("{id:guid}")]
        [Authorize]
        [RequireVerifiedEmail]
        [RequireAdmin]
        public async Task<IActionResult> Handle(Guid id, [FromBody] UpdateProductRequest request)
        {
            // A diferencia del GetProductById, acá NO filtramos por IsActive.
            // Un admin debería poder editar incluso productos inactivos —
            // por ejemplo, para reactivarlos o actualizar su información.
            var product = await db.Products.FindAsync(id);
            if (product is null)
                return NotFound(new { message = "Producto no encontrado" });

            if (request.CategoryId.HasValue)
            {
                var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value);
                if (!categoryExists)
                    return BadRequest(new { message = "La categoría especificada no existe" });
            }

            // Solo actualizamos los campos que el cliente envió.
            // El operador ??= significa "asignate este valor solo si el
            // lado izquierdo tiene un valor nuevo" — preservamos el valor
            // existente para los campos que no se enviaron en el request.
            if (request.Name is not null) product.Name = request.Name;
            if (request.Description is not null) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.Stock.HasValue) product.Stock = request.Stock.Value;
            if (request.ImageUrl is not null) product.ImageUrl = request.ImageUrl;
            if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;
            if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Ok(product);
        }
    }
}
