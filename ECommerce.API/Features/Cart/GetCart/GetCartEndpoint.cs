using ECommerce.API.Common.Auth;
using ECommerce.API.Common.Auth.Filters;
using ECommerce.API.Common.Data;
using ECommerce.API.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Cart.GetCart
{
    [Route("cart")]
    public class GetCartEndpoint(AppDbContext db, IJwtService jwtService)
    : BaseEndpoint(db, jwtService)
    {
        [HttpGet]
        [Authorize]
        [RequireVerifiedEmail]
        public async Task<IActionResult> Handle()
        {
            var user = await GetCurrentUserAsync();
            if (user is null) return Unauthorized();

            // Buscamos el carrito del usuario con todos sus items y los productos asociados.
            // Si no tiene carrito todavía, devolvemos uno vacío "virtual" sin crear nada
            // en la DB — el carrito solo se crea cuando el usuario agrega el primer item.
            var cart = await db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart is null)
            {
                return Ok(new CartResponse(Guid.Empty, [], 0, 0));
            }

            var items = cart.Items
                .Where(i => i.Product.IsActive) // filtramos items cuyo producto fue desactivado
                .Select(i => new CartItemResponse(
                    i.Id,
                    i.ProductId,
                    i.Product.Name,
                    i.Product.ImageUrl,
                    i.Product.Price,
                    i.Quantity,
                    i.Product.Price * i.Quantity
                ));

            var total = items.Sum(i => i.Subtotal);
            var itemCount = items.Sum(i => i.Quantity);

            return Ok(new CartResponse(cart.Id, items, total, itemCount));
        }
    }
}
