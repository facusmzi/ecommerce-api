using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Cart.UpdateItem
{
    [Route("cart/items")]
    public class UpdateItemEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpPut("{itemId:guid}")]
        [Authorize]
        [RequireVerifiedEmail]
        public async Task<IActionResult> Handle(Guid itemId, [FromBody] UpdateItemRequest request)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            // Buscamos el item verificando que pertenezca al carrito del usuario.
            // Este join es importante por seguridad — sin él, un usuario podría
            // modificar items del carrito de otro usuario si adivinara el itemId.
            var item = await db.CartItems
                .Include(i => i.Cart)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == user.Id);

            if (item is null)
                return NotFound(new { message = "Item no encontrado" });

            if (item.Product.Stock < request.Quantity)
                return BadRequest(new { message = $"Stock insuficiente. Disponible: {item.Product.Stock}" });

            item.Quantity = request.Quantity;
            item.Cart.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return Ok(new { message = "Cantidad actualizada" });
        }
    }
}
