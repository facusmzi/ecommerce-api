using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Cart.RemoveItem
{
    [Route("cart/items")]
    public class RemoveItemEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpDelete("{itemId:guid}")]
        [Authorize]
        [RequireVerifiedEmail]
        public async Task<IActionResult> Handle(Guid itemId)
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            // Igual que en UpdateItem, verificamos que el item pertenezca
            // al carrito del usuario antes de permitir la operación.
            var item = await db.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == user.Id);

            if (item is null)
                return NotFound(new { message = "Item no encontrado" });

            db.CartItems.Remove(item);
            item.Cart.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return NoContent();
        }
    }
}
